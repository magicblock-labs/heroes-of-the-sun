import { expect } from "chai";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { PlayerWrapper } from "./wrappers/player.wrapper";
import { SettlementWrapper } from "./wrappers/settlement.wrapper";
import { LocationAllocatorWrapper } from "./wrappers/location_allocator.wrapper";


describe("Creates A Player And Assigns a Settlement", async () => {

  const player = new PlayerWrapper();
  const world = new WorldWrapper();
  const settlement = new SettlementWrapper();
  const locationAllocator = new LocationAllocatorWrapper();

  it("Initializes a player", async () => {
    await player.init(await world.getWorldPda());
    await locationAllocator.init(await world.getWorldPda())

    const state = await player.state();
    expect(state.settlements.length).to.eq(0);
  });


  it("Assigns settlement to a player", async () => {
    const allocatorState = (await locationAllocator.state());
    console.log("!", allocatorState);
    await settlement.init(await world.getWorldPda(), allocatorState.currentX, allocatorState.currentY)
    console.log("!", await settlement.state());
    const state = await player.assignSettlement(
      settlement.entityPda,
      settlement.settlementComponent.programId,
      locationAllocator.entityPda,
      locationAllocator.locationAllocatorComponent.programId,
    );
    expect(state.settlements.length).to.gt(0);
  });


  after(async () => {
    console.log(await player.state());
    console.log(await settlement.state());
    console.log(await locationAllocator.state());
  })
});

