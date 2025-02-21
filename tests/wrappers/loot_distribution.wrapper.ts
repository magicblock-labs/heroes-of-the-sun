import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { AccountMeta, PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  ApplySystem,
  InitializeComponent,
} from "@magicblock-labs/bolt-sdk"
import { Lootdistribution } from "../../target/types/lootdistribution";
import { ClaimLoot } from "../../target/types/claim_loot";



export type ClaimLootArgs = {
  index: number
}

export class LootDistributionWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  lootDistributionComponent: Program<Lootdistribution>;
  claimLootSystem: Program<ClaimLoot>;

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
        seed: Buffer.from("hots_loot_distribution")
      });

      this.lootDistributionComponent = anchor.workspace.Lootdistribution as Program<Lootdistribution>;

      let txSign = await this.provider.sendAndConfirm(lootEntity.transaction);
      this.entityPda = lootEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${lootEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: this.lootDistributionComponent.programId
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the looy component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.lootDistributionComponent.account.lootDistribution.fetch(this.componentPda);
  }


  async claimLoot(args: ClaimLootArgs, extraAccounts: AccountMeta[]) {
    // Run the claim system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.claimLootSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.lootDistributionComponent.programId }],
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
