import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  InitializeNewWorld,
  AddEntity,
  InitializeComponent,
  ApplySystem,
  Component,
  System,
} from "../../../bolt/clients/typescript/lib"
import { AssignSettlement } from "../../target/types/assign_settlement";
import { Player } from "../../target/types/player";
import { EcsBundle } from "../../target/types/ecs_bundle";


export class PlayerWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  bundle: Program<EcsBundle>;
  assignSettlementSystem: Program<AssignSettlement>;

  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const playerEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
      });

      this.bundle = anchor.workspace.EcsBundle as Program<EcsBundle>;
      this.assignSettlementSystem = anchor.workspace.AssignSettlement as Program<AssignSettlement>;
      
      let txSign = await this.provider.sendAndConfirm(playerEntity.transaction);
      this.entityPda = playerEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${playerEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: new Component(this.bundle.programId, "player"),
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the settlement component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.bundle.account.player.fetch(this.componentPda);
  }

  async assignSettlement(settlementPDA: PublicKey, settlementID: Component, allocatorPDA: PublicKey, allocatorID: Component) {

    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.assignSettlementSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: new Component(this.bundle.programId, "player") }],
      },
      {
        entity: settlementPDA,
        components: [{ componentId: settlementID }],
      },
      {
        entity: allocatorPDA,
        components: [{ componentId: allocatorID }],
      }],
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction, null, { skipPreflight: true });
    console.log(`assignSettlement tx: ${txSign}`);

    return await this.state();
  }


  async assignHero(heroPDA: PublicKey, heroID: Component) {

    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: new System(this.bundle.programId, "assign_hero"),
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: new Component(this.bundle.programId, "player") }],
      },
      {
        entity: heroPDA,
        components: [{ componentId: heroID }],
      }],
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`assignHero tx: ${txSign}`);

    return await this.state();
  }
};
