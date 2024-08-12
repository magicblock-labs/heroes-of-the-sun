import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  InitializeNewWorld,
  AddEntity,
  InitializeComponent,
  ApplySystem,
} from "@magicblock-labs/bolt-sdk"
import { Settlement } from "../target/types/settlement";
import { Wait } from "../target/types/wait";
import { Build } from "../target/types/build";
import { AssignLabour } from "../target/types/assign_labour";
import { Upgrade } from "../target/types/upgrade";


export type BuildArgs = {
  x: number, y: number, config_index: number
}

export type AssignLabourArgs = {
  building_index: number, labour_index: number
}

export type UpgradeArgs = {
  index: number
}

export type WaitArgs = {
  time: number
}

export class SettlementWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;


  settlementComponent: Program<Settlement>;
  waitSystem: Program<Wait>;
  buildSystem: Program<Build>;
  assignLabourSystem: Program<AssignLabour>;
  upgradeSystem: Program<Upgrade>;

  async init() {
    this.provider = anchor.AnchorProvider.env();
    anchor.setProvider(this.provider);

    this.settlementComponent = anchor.workspace.Settlement as Program<Settlement>;
    this.waitSystem = anchor.workspace.Wait as Program<Wait>;
    this.buildSystem = anchor.workspace.Build as Program<Build>;
    this.assignLabourSystem = anchor.workspace.AssignLabour as Program<AssignLabour>;
    this.upgradeSystem = anchor.workspace.Upgrade as Program<Upgrade>;

    const initNewWorld = await InitializeNewWorld({
      payer: this.provider.wallet.publicKey,
      connection: this.provider.connection,
    });
    let txSign = await this.provider.sendAndConfirm(initNewWorld.transaction);
    this.worldPda = initNewWorld.worldPda;
    console.log(`Initialized a new world (ID=${this.worldPda}). Initialization signature: ${txSign}`);

    const addEntity = await AddEntity({
      payer: this.provider.wallet.publicKey,
      world: this.worldPda,
      connection: this.provider.connection,
    });
    txSign = await this.provider.sendAndConfirm(addEntity.transaction);
    this.entityPda = addEntity.entityPda;
    console.log(`Initialized a new Entity (PDA=${addEntity.entityPda}). Initialization signature: ${txSign}`);

    const initializeComponent = await InitializeComponent({
      payer: this.provider.wallet.publicKey,
      entity: this.entityPda,
      componentId: this.settlementComponent.programId,
    });
    txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
    this.componentPda = initializeComponent.componentPda;
    console.log(`Initialized the settlement component. Initialization signature: ${txSign}`);

    return await this.state();

  }

  async state() {
    return await this.settlementComponent.account.settlement.fetch(this.componentPda);
  }

  async build(args: BuildArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      authority: this.provider.wallet.publicKey,
      systemId: this.buildSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.settlementComponent.programId }],
      }],
      args
    });
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`build tx: ${txSign}`);

    return await this.state();
  }

  async assignLabour(args: AssignLabourArgs) {
    const applySystem = await ApplySystem({
      authority: this.provider.wallet.publicKey,
      systemId: this.assignLabourSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.settlementComponent.programId }],
      }],
      args
    }
    );
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`assignLabour: ${txSign}`);

    return await this.state();
  }


  async upgrade(args: UpgradeArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      authority: this.provider.wallet.publicKey,
      systemId: this.upgradeSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.settlementComponent.programId }],
      }],
      args
    });
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`build tx: ${txSign}`);

    return await this.state();
  }

  async wait(args: WaitArgs) {

    // Run the movement system
    const applySystem = await ApplySystem({
      authority: this.provider.wallet.publicKey,
      systemId: this.waitSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.settlementComponent.programId }],
      }],
      args
    }
    );
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`Waited a day: ${txSign}`);

    return await this.state();
  }
};
