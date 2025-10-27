import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { AccountMeta, PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  ApplySystem,
  Component,
  InitializeComponent,
} from "../../../bolt/clients/typescript/lib"
import { LootDistribution } from "../../target/types/loot_distribution";
import { ClaimLoot } from "../../target/types/claim_loot";
import { EcsBundle } from "../../target/types/ecs_bundle";



export type ClaimLootArgs = {
  index: number
}

export class LootDistributionWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  claimLootSystem: Program<ClaimLoot>;
  bundle: Program<EcsBundle>;

  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    this.claimLootSystem = anchor.workspace.ClaimLoot as Program<ClaimLoot>;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const lootEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
        seed: new Uint8Array(Buffer.from("hots_loot_distribution"))
      });

      this.bundle = anchor.workspace.EcsBundle as Program<EcsBundle>;

      let txSign = await this.provider.sendAndConfirm(lootEntity.transaction);
      this.entityPda = lootEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${lootEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: new Component(this.bundle.programId, "loot_distribution")
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the loot distribution component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.bundle.account.lootDistribution.fetch(this.componentPda);
  }


  async claimLoot(args: ClaimLootArgs, extraAccounts: AccountMeta[]) {
    // Run the claim system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.claimLootSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: new Component(this.bundle.programId, "loot_distribution") }],
      }],
      extraAccounts: [
        {
          pubkey: this.provider.wallet.publicKey,
          isWritable: true,
          isSigner: true,
        },
      ].concat(extraAccounts),
      args,
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`claimLoot tx: ${txSign}`);

    return await this.state();
  }

};
