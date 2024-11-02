// import { assert, expect } from "chai";
// import { BuildingType, SettlementWrapper } from "./wrappers/settlement.wrapper";
// import { WorldWrapper } from "./wrappers/world.wrapper";

// describe("Test suite for: build area and cost", () => {

//   const settlement = new SettlementWrapper();
//   const world = new WorldWrapper();

//   it("Initializes (with a default town hall) if needed", async () => {
//     const state = await settlement.reset(await world.getWorldPda());
//     expect(state.buildings.length).to.eq(1);
//   });

//   it("Builds a building", async () => {
//     var stateBefore = (await settlement.state());
//     const state = await settlement.build({ x: 1, y: 1, config_index: BuildingType.WaterCollector, worker_index: 0 });
//     expect(state.buildings.length).to.gt(1);
//     expect(state.treasury.wood).to.lt(stateBefore.treasury.wood)
//   });

//   it("Builds another building", async () => {
//     var stateBefore = (await settlement.state());
//     const state = await settlement.build({ x: 1, y: 10, config_index: BuildingType.FoodCollector, worker_index: 0 });
//     expect(state.buildings.length).to.gt(1);
//     expect(state.treasury.wood).to.lt(stateBefore.treasury.wood)
//   });

//   it("Prevents building overlap", async () => {
//     try {
//       await settlement.build({ x: 1, y: 1, config_index: BuildingType.WaterCollector, worker_index: 0 });
//       assert(false, "should've failed but didn't ")
//     } catch (_err) {
//       //why cant in just get the error from the _err type??? error types are not exposed :/
//       expect(new String(_err)).to.contain("6000")
//     }
//   });

//   after(async () => {
//     console.log(await settlement.state());
//   })
// });
