import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  InitializeComponent,
} from "@magicblock-labs/bolt-sdk"
import { Lootdistribution } from "../../target/types/lootdistribution";



export class LootDistributionWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  lootDistributionComponent: Program<Lootdistribution>;

  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const lootEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
        seed: "hots_loot_distribution"
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

};
