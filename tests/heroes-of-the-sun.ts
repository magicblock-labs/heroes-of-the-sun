import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  InitializeNewWorld,
  AddEntity,
  InitializeComponent,
  ApplySystem,
} from "@magicblock-labs/bolt-sdk"
import { assert, expect } from "chai";
import { Settlement } from "../target/types/settlement";
import { Time } from "../target/types/time";
import { Build } from "../target/types/build";
import { Labour } from "../target/types/labour";


type BuildArgs = {
  x: number, y: number, id: number
}

type AllocateLabourArgs = {
  building: number, labour: number
}

type WaitArgs = {
  days: number
}


describe("HeroesOfTheSun", () => {
  // Configure the client to use the local cluster.
  const provider = anchor.AnchorProvider.env();
  anchor.setProvider(provider);

  // Constants used to test the program.
  let worldPda: PublicKey;
  let entityPda: PublicKey;
  let componentPda: PublicKey;

  const settlementComponent = anchor.workspace.Settlement as Program<Settlement>;
  const timeSystem = anchor.workspace.Time as Program<Time>;
  const buildSystem = anchor.workspace.Build as Program<Build>;
  const labourSystem = anchor.workspace.Labour as Program<Labour>;


  const build = async (args: BuildArgs) => {
    // Run the build system
    const applySystem = await ApplySystem({
      authority: provider.wallet.publicKey,
      systemId: buildSystem.programId,
      entities: [{
        entity: entityPda,
        components: [{ componentId: settlementComponent.programId }],
      }],
      args
    });
    const txSign = await provider.sendAndConfirm(applySystem.transaction);
    console.log(`build tx: ${txSign}`);

    return await settlementComponent.account.settlement.fetch(componentPda);
  }

  const assignLabour = async (args: AllocateLabourArgs) => {
    const applySystem = await ApplySystem({
      authority: provider.wallet.publicKey,
      systemId: labourSystem.programId,
      entities: [{
        entity: entityPda,
        components: [{ componentId: settlementComponent.programId }],
      }],
      args: {
        labour: 0,
        building: 0
      }
    }
    );
    const txSign = await provider.sendAndConfirm(applySystem.transaction);
    return await settlementComponent.account.settlement.fetch(componentPda);
  }

  const wait = async (args: WaitArgs) => {

    // Run the movement system
    const applySystem = await ApplySystem({
      authority: provider.wallet.publicKey,
      systemId: timeSystem.programId,
      entities: [{
        entity: entityPda,
        components: [{ componentId: settlementComponent.programId }],
      }],
      args
    }
    );
    const txSign = await provider.sendAndConfirm(applySystem.transaction);
    console.log(`Waited a day: ${txSign}`);

    return await settlementComponent.account.settlement.fetch(componentPda);

  }

  it("Initializes a NewWorld", async () => {
    const initNewWorld = await InitializeNewWorld({
      payer: provider.wallet.publicKey,
      connection: provider.connection,
    });
    const txSign = await provider.sendAndConfirm(initNewWorld.transaction);
    worldPda = initNewWorld.worldPda;
    console.log(`Initialized a new world (ID=${worldPda}). Initialization signature: ${txSign}`);
  });

  it("Adds an entity", async () => {
    const addEntity = await AddEntity({
      payer: provider.wallet.publicKey,
      world: worldPda,
      connection: provider.connection,
    });
    const txSign = await provider.sendAndConfirm(addEntity.transaction);
    entityPda = addEntity.entityPda;
    console.log(`Initialized a new Entity (PDA=${addEntity.entityPda}). Initialization signature: ${txSign}`);
  });

  it("Creates a settlement", async () => {
    const initializeComponent = await InitializeComponent({
      payer: provider.wallet.publicKey,
      entity: entityPda,
      componentId: settlementComponent.programId,
    });
    const txSign = await provider.sendAndConfirm(initializeComponent.transaction);
    componentPda = initializeComponent.componentPda;
    console.log(`Initialized the grid component. Initialization signature: ${txSign}`);
  });

  it("Adds a building", async () => {
    const state = await build({ x: 1, y: 1, id: 1 });
    expect(state.buildings.length).to.gt(0);
  });

  it("Prevents overlap", async () => {
    try {
      await build({ x: 1, y: 1, id: 1 });
      assert(false, "should've failed but didn't ")
    } catch (_err) {
      //why cant in just get the error from the _err type??? error types are not exposed :/
      expect(new String(_err)).to.contain("6000")
    }
  });

  it("Allocates Labour", async () => {
    const state = await assignLabour({ labour: 0, building: 0 });
    console.log(state);
  });

  it("Waits a day", async () => {
    // Check that the system has been applied and x is > 0
    const state = await wait({ days: 5 });
    console.log(state);
  });

});
