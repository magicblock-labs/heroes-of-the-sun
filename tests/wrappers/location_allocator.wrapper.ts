import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  InitializeComponent,
} from "@magicblock-labs/bolt-sdk"
import { Locationallocator } from "../../target/types/locationallocator";



export class LocationAllocatorWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  locationAllocatorComponent: Program<Locationallocator>;

  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const allocatorEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
        seed: Buffer.from("hots_allocator")
      });

      this.locationAllocatorComponent = anchor.workspace.Locationallocator as Program<Locationallocator>;

      let txSign = await this.provider.sendAndConfirm(allocatorEntity.transaction);
      this.entityPda = allocatorEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${allocatorEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: this.locationAllocatorComponent.programId
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the settlement component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.locationAllocatorComponent.account.locationAllocator.fetch(this.componentPda);
  }

};
