import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  InitializeComponent,
  ApplySystem,
  Component,
  DelegateComponent,
  System,
} from "../../../bolt/clients/typescript/lib"
import { EcsBundle } from "../../target/types/ecs_bundle";

export type MoveHeroArgs = {
  x: number,
  y: number
}

export type ChangeBackpackArgs = {

  food: number,
  water: number,
  wood: number,
  stone: number,
}


export class HeroWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  bundle: Program<EcsBundle>;
  
  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const heroEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
      });

      this.bundle = anchor.workspace.EcsBundle as Program<EcsBundle>;

      let txSign = await this.provider.sendAndConfirm(heroEntity.transaction);
      this.entityPda = heroEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${heroEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: new Component(this.bundle.programId, "hero"),
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the hero component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.bundle.account.hero.fetch(this.componentPda);
  }

  async moveHero(args: MoveHeroArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: new System(this.bundle.programId, "move_hero"),
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: new Component(this.bundle.programId, "hero") }],
      }],
      args
    });
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`build tx: ${txSign}`);

    return await this.state();
  }


  async delegate() {


    const delegateIx = await DelegateComponent({
      payer: this.provider.wallet.publicKey,
      entity: this.entityPda,
      componentId: new Component(this.bundle.programId, "hero"),
    });
    const tx = new anchor.web3.Transaction().add(delegateIx.instruction);
    tx.feePayer = this.provider.wallet.publicKey;
    tx.recentBlockhash = (await this.provider.connection.getLatestBlockhash()).blockhash;
    const txSign = await this.provider.sendAndConfirm(tx, [], { commitment: "confirmed" });
    console.log(
      `Delegation signature: ${txSign}`
    );


    return await this.state();
  }


  async changeBackpack(settlementPDA: PublicKey, settlementID: Component, args: ChangeBackpackArgs) {
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: new System(this.bundle.programId, "change_backpack"),
      entities: [
        {
          entity: this.entityPda,
          components: [{ componentId: new Component(this.bundle.programId, "hero") }],
        },
        {
          entity: settlementPDA,
          components: [{ componentId: settlementID }],
        }],
      args,
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`backpack tx: ${txSign}`);

    return await this.state();
  }

};
