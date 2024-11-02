import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  InitializeComponent,
  ApplySystem,
} from "@magicblock-labs/bolt-sdk"
import { Locationallocator } from "../../target/types/locationallocator";
import { BumpLocationAllocator } from "../../target/types/bump_location_allocator";



export class LocationAllocatorWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  locationAllocatorComponent: Program<Locationallocator>;
  bumpLocationAllocatorSystem: Program<BumpLocationAllocator>;

  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const allocatorEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
        seed: "hots_allocator"
      });

      this.locationAllocatorComponent = anchor.workspace.Locationallocator as Program<Locationallocator>;
      this.bumpLocationAllocatorSystem = anchor.workspace.BumpLocationAllocator as Program<BumpLocationAllocator>;

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

  async bump() {

    // Run the build system
    const applySystem = await ApplySystem({
      authority: this.provider.wallet.publicKey,
      systemId: this.bumpLocationAllocatorSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.locationAllocatorComponent.programId }],
      }]
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`build tx: ${txSign}`);

    return await this.state();
  }

};
