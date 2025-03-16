import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  ApplySystem,
  InitializeComponent,
} from "@magicblock-labs/bolt-sdk"
import { SmartObjectLocation } from "../../target/types/smart_object_location";
import { SmartObjectDeity } from "../../target/types/smart_object_deity";
import { SmartObjectInit } from "../../target/types/smart_object_init";



export type SmartObjectInitArgs = {
  x: number,
  y: number,
  entity: number[]
}

export type DeityInteractionArgs = {
  index: number
}

export class SmartObjectWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  locationComponentPda: PublicKey;
  deityComponentPda: PublicKey;

  smartObjectLocationComponent: Program<SmartObjectLocation>;
  smartObjectDeityComponent: Program<SmartObjectDeity>;

  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    if (!this.locationComponentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const smartObjectEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
        seed: new Uint8Array(Buffer.from("hots_smart_object_test2"))
      });

      this.smartObjectLocationComponent = anchor.workspace.SmartObjectLocation as Program<SmartObjectLocation>;
      this.smartObjectDeityComponent = anchor.workspace.SmartObjectDeity as Program<SmartObjectDeity>;

      let txSign = await this.provider.sendAndConfirm(smartObjectEntity.transaction);
      this.entityPda = smartObjectEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${smartObjectEntity.entityPda}). Initialization signature: ${txSign}`);

      let initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: this.smartObjectLocationComponent.programId
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.locationComponentPda = initializeComponent.componentPda;
      console.log(`Initialized the smart object location component. Initialization signature: ${txSign}`);

      initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: this.smartObjectDeityComponent.programId
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.deityComponentPda = initializeComponent.componentPda;
      console.log(`Initialized the smart object functional component. Initialization signature: ${txSign}`);
    }
  }

  async location() {
    return await this.smartObjectLocationComponent.account.smartObjectLocation.fetch(this.locationComponentPda);
  }


  async deity() {
    return await this.smartObjectDeityComponent.account.smartObjectDeity.fetch(this.deityComponentPda);
  }

  async initObj(args: SmartObjectInitArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: (anchor.workspace.SmartObjectInit as Program<SmartObjectInit>).programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.smartObjectLocationComponent.programId }],
      }],
      args
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`assignHero tx: ${txSign}`);

    return await this.deity();
  }


  async interact(args: DeityInteractionArgs, heroPDA: PublicKey, heroProgramID: PublicKey) {

    var deity = await this.deity();
    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: deity.system,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.smartObjectDeityComponent.programId }],
      },
      {
        entity: heroPDA,
        components: [{ componentId: heroProgramID }],
      }],
      args
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`assignHero tx: ${txSign}`);

    return await this.deity();
  }

};
