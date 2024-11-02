import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  InitializeNewWorld,
  AddEntity,
  InitializeComponent,
  ApplySystem,
} from "@magicblock-labs/bolt-sdk"
import { AssignSettlement } from "../../target/types/assign_settlement";
import { Player } from "../../target/types/player";



export type AssignSettlementArgs = {
  location_x: number,
  location_y: number
}

export class PlayerWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  playerComponent: Program<Player>;
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

      this.playerComponent = anchor.workspace.Player as Program<Player>;
      this.assignSettlementSystem = anchor.workspace.AssignSettlement as Program<AssignSettlement>;

      let txSign = await this.provider.sendAndConfirm(playerEntity.transaction);
      this.entityPda = playerEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${playerEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: this.playerComponent.programId,
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the settlement component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.playerComponent.account.player.fetch(this.componentPda);
  }

  async assignSettlement(args: AssignSettlementArgs) {

    // Run the build system
    const applySystem = await ApplySystem({
      authority: this.provider.wallet.publicKey,
      systemId: this.assignSettlementSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.playerComponent.programId }],
      }],
      args
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`build tx: ${txSign}`);

    return await this.state();
  }

};
