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
    // Check that the component has been initialized and x is 0
    const settlementBefore = await settlementComponent.account.settlement.fetch(
      componentPda
    );
    expect(settlementBefore.buildings.length).to.equal(0);

    // Run the build system
    const applySystem = await ApplySystem({
      authority: provider.wallet.publicKey,
      systemId: buildSystem.programId,
      entities: [{
        entity: entityPda,
        components: [{ componentId: settlementComponent.programId }],
      }],
      args: {
        x: 1, y: 1, id: 0
      }
    });
    const txSign = await provider.sendAndConfirm(applySystem.transaction);
    console.log(`Added a building: ${txSign}`);

    // Check that the system has been applied and x is > 0
    const settlementAfter = await settlementComponent.account.settlement.fetch(
      componentPda
    );

    console.log(settlementAfter);

    expect(settlementAfter.buildings.length).to.gt(0);
  });



  it("Prevents overlap", async () => {

    try {

      // Run the build system
      const applySystem = await ApplySystem({
        authority: provider.wallet.publicKey,
        systemId: buildSystem.programId,
        entities: [{
          entity: entityPda,
          components: [{ componentId: settlementComponent.programId }],
        }],
        args: {
          x: 1, y: 1, id: 0
        }
      });
      const txSign = await provider.sendAndConfirm(applySystem.transaction);

      assert(false, "should've failed but didn't ")
    } catch (_err) {
      //why cant in just get the error from the _err type??? error types are not exposed :/
      expect(new String(_err)).to.contain("6000")
    }
  });

  it("Waits a day", async () => {
    // Check that the component has been initialized and x is 0
    const settlementBefore = await settlementComponent.account.settlement.fetch(
      componentPda
    );
    expect(settlementBefore.day).to.equal(0);

    // Run the movement system
    const applySystem = await ApplySystem({
      authority: provider.wallet.publicKey,
      systemId: timeSystem.programId,
      entities: [{
        entity: entityPda,
        components: [{ componentId: settlementComponent.programId }],
      }]
    }
    );
    const txSign = await provider.sendAndConfirm(applySystem.transaction);
    console.log(`Waited a day: ${txSign}`);

    // Check that the system has been applied and x is > 0
    const settlementAfter = await settlementComponent.account.settlement.fetch(
      componentPda
    );
    expect(settlementAfter.day).to.gt(0);
  });

});
