import { expect } from "chai";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { PlayerWrapper } from "./wrappers/player.wrapper";
import { SettlementWrapper } from "./wrappers/settlement.wrapper";
import { LocationAllocatorWrapper } from "./wrappers/location_allocator.wrapper";
import { TokenWrapper } from "./wrappers/token.wrapper";



describe("Creates A Player And Assigns a Settlement", async () => {

  const player = new PlayerWrapper();
  const world = new WorldWrapper();
  const settlement = new SettlementWrapper();
  const locationAllocator = new LocationAllocatorWrapper();
  const token = new TokenWrapper();



  it("creates a token", async () => {
    await token.createToken();//
  });

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


  // it("Waits a move and mints a token", async () => {
  //   let state = await settlement.wait({ time: 1 }, token.getMintExtraAccounts());

  //   console.log("!", await settlement.state());

  //   expect(state.faith).to.gt(0);
  // });

  // it("Researches and burns a token", async () => {
  //   let state = await settlement.research({ research_type: 1 }, token.getBurnExtraAccounts());

  //   console.log("!", await settlement.state());

  //   expect(state.faith).to.gt(0);
  // });


  after(async () => {
    console.log(await player.state());
    console.log(await settlement.state());
    console.log(await locationAllocator.state());
    console.log(await locationAllocator.state());
    // console.log(await token.provider.connection.getTokenAccountBalance(token.associatedTokenAccountAddress));
  })
});

