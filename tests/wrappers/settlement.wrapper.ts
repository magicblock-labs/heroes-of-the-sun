import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  InitializeNewWorld,
  AddEntity,
  InitializeComponent,
  ApplySystem,
} from "@magicblock-labs/bolt-sdk"
import { AssignWorker } from "../../target/types/assign_worker";
import { Build } from "../../target/types/build";
import { Repair } from "../../target/types/repair";
import { Research } from "../../target/types/research";
import { Reset } from "../../target/types/reset";
import { Settlement } from "../../target/types/settlement";
import { Upgrade } from "../../target/types/upgrade";
import { Wait } from "../../target/types/wait";


export enum BuildingType {
  TownHall,
  Altar,
  Research,
  FoodCollector,
  FoodStorage,
  WoodCollector,
  WoodStorage,
  WaterCollector,
  WaterStorage,
  StoneCollector,
  StoneStorage,
  GoldCollector,
  GoldStorage
}


export type BuildArgs = {
  x: number, y: number, config_index: BuildingType, worker_index: number
}

export type AssignWorkerArgs = {
  building_index: number, worker_index: number
}

export type UpgradeArgs = {
  index: number,
  worker_index: number
}

export type RepairArgs = {
  index: number
}

export type ResearchArgs = {
  research_type: number
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
  assignWorkerSystem: Program<AssignWorker>;
  upgradeSystem: Program<Upgrade>;
  repairSystem: Program<Repair>;
  researchSystem: Program<Research>;
  resetSystem: Program<Reset>;

  async init(worldPda: PublicKey, x: number, y: number) {

    this.worldPda = worldPda;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      this.settlementComponent = anchor.workspace.Settlement as Program<Settlement>;
      this.waitSystem = anchor.workspace.Wait as Program<Wait>;
      this.buildSystem = anchor.workspace.Build as Program<Build>;
      this.assignWorkerSystem = anchor.workspace.AssignWorker as Program<AssignWorker>;
      this.upgradeSystem = anchor.workspace.Upgrade as Program<Upgrade>;
      this.repairSystem = anchor.workspace.Repair as Program<Repair>;
      this.researchSystem = anchor.workspace.Research as Program<Research>;
      this.resetSystem = anchor.workspace.Reset as Program<Reset>;

      const addEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
        seed: `${x}x${y}`
      });
      let txSign = await this.provider.sendAndConfirm(addEntity.transaction);
      this.entityPda = addEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${addEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: this.settlementComponent.programId,
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction, [], { skipPreflight: true });
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the settlement component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.settlementComponent.account.settlement.fetch(this.componentPda);
  }

  async build(args: BuildArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
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

  async assignWorker(args: AssignWorkerArgs) {
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.assignWorkerSystem.programId,
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
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.upgradeSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.settlementComponent.programId }],
      }],
      args
    });
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`upgrade tx: ${txSign}`);

    return await this.state();
  }

  async repair(args: RepairArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.repairSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.settlementComponent.programId }],
      }],
      args
    });
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`repair tx: ${txSign}`);

    return await this.state();
  }

  async research(args: ResearchArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.researchSystem.programId,
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
      world: this.worldPda,
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
    console.log(`Waited ${args.time} turns: ${txSign}`);

    return await this.state();
  }


  async reset() {

    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.resetSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.settlementComponent.programId }],
      }]
    }
    );
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`Reset: ${txSign}`);

    return await this.state();
  }

  async upgradeAndWait(index: number, worker_index: number) {
    let state = await this.upgrade({ index, worker_index });
    return await this.wait({ time: this.getTurnsToCompleteAll(state) });
  }

  getTurnsToCompleteAll(state: { buildings: { turnsToBuild: number; }[]; }): number {
    let result = 0;

    for (let building of state.buildings)
      if (building.turnsToBuild > result)
        result = building.turnsToBuild;

    return result;
  }

};
