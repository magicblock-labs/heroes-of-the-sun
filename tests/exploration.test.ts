import { assert, expect } from "chai";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { PlayerWrapper } from "./wrappers/player.wrapper";
import { SettlementWrapper } from "./wrappers/settlement.wrapper";
import { LocationAllocatorWrapper } from "./wrappers/location_allocator.wrapper";
import { TokenWrapper } from "./wrappers/token.wrapper";
import { HeroWrapper } from "./wrappers/hero.wrapper";
import { LootDistributionWrapper } from "./wrappers/loot_distribution.wrapper";



// describe("Exploration and multiplayer tests", async () => {

//   const player = new PlayerWrapper();
//   const hero = new HeroWrapper();
  const world = new WorldWrapper();
//   const settlement = new SettlementWrapper();
const locationAllocator = new LocationAllocatorWrapper();
//   const lootDistribution = new LootDistributionWrapper();


it("Initializes a player", async () => {
    // await player.init(await world.getWorldPda());
    await locationAllocator.init(await world.getWorldPda())

    const state = await locationAllocator.state();
    expect(state.currentX).to.eq(0);
});

//   it("Assigns settlement to a player", async () => {
//     const allocatorState = (await locationAllocator.state());
//     await settlement.init(await world.getWorldPda(), allocatorState.currentX, allocatorState.currentY)
//     const state = await player.assignSettlement(
//       settlement.entityPda,
//       settlement.settlementComponent.programId,
//       locationAllocator.entityPda,
//       locationAllocator.locationAllocatorComponent.programId,
//     );
//     expect(state.settlements.length).to.gt(0);
//   });

//   it("Assigns hero to a player", async () => {
//     await hero.init(await world.getWorldPda())
//     await player.assignHero(
//       hero.entityPda,
//       hero.heroComponent.programId,
//     );
//     const state = await hero.state();
//     expect(state.owner.toString()).to.not.be.null;
//   });

//   it("Moves hero to a pos", async () => {
//     const state = await hero.moveHero({ x: 1, y: 1 });
//     expect(state.x).to.eq(1);
//     expect(state.y).to.eq(1);
//   });


//   it("Inits loot distribution", async () => {

//     await lootDistribution.init(await world.getWorldPda());
//     const lootState = await lootDistribution.state();
//     expect(lootState.loots.length).gt(0);
//   });

//   it("Fails to collects Loot with wrong index", async () => {

//     const lootState = await lootDistribution.state();
//     await hero.moveHero(lootState.loots[0]);


//     try {
//       await hero.claimLoot(
//         settlement.entityPda,
//         settlement.settlementComponent.programId,
//         lootDistribution.entityPda,
//         lootDistribution.lootDistributionComponent.programId,
//         { index: 1 });

//       assert(false, "should've failed but didn't ")
//     } catch (_err) {
//       //why cant in just get the error from the _err type??? error types are not exposed :/
//       expect(new String(_err)).to.contain("6001")
//     }
//   });

//   it("Fails to collects Loot at wrong location", async () => {

//     await lootDistribution.init(await world.getWorldPda());
//     const lootState = await lootDistribution.state();
//     await hero.moveHero({ x: lootState.loots[0].x, y: lootState.loots[0].y + 1 });

//     try {
//       await hero.claimLoot(
//         settlement.entityPda,
//         settlement.settlementComponent.programId,
//         lootDistribution.entityPda,
//         lootDistribution.lootDistributionComponent.programId,
//         { index: 1 });

//       assert(false, "should've failed but didn't ")
//     } catch (_err) {
//       //why cant in just get the error from the _err type??? error types are not exposed :/
//       expect(new String(_err)).to.contain("6001")
//     }
//   });

//   it("Collects Loot", async () => {

//     await lootDistribution.init(await world.getWorldPda());
//     const lootState = await lootDistribution.state();
//     await hero.moveHero(lootState.loots[0]);

//     const settlementBefore = await settlement.state();

//     await hero.claimLoot(
//       settlement.entityPda,
//       settlement.settlementComponent.programId,
//       lootDistribution.entityPda,
//       lootDistribution.lootDistributionComponent.programId,
//       { index: 0 });

//     const settlementAfter = await settlement.state();

//     expect(settlementAfter.treasury.wood).to.gt(settlementBefore.treasury.wood);
//   });

//   it("Puts stuff into backpack", async () => {

//     const settlementBefore = await settlement.state();
//     const heroBefore = await hero.state();

//     await hero.changeBackpack(settlement.entityPda,
//       settlement.settlementComponent.programId, { water: 0, food: 0, wood: 1, stone: 0 });


//     const settlementAfter = await settlement.state();
//     const heroAfter = await hero.state();

//     expect(settlementAfter.treasury.wood).to.lt(settlementBefore.treasury.wood);
//     expect(heroAfter.backpack.wood).to.gt(heroBefore.backpack.wood);
//   });



//   it("Doesnt put stuff into backpack above cap", async () => {


//     try {

//       await hero.changeBackpack(settlement.entityPda,
//         settlement.settlementComponent.programId, { water: 0, food: 0, wood: 10, stone: 0 });

//       assert(false, "should've failed but didn't ")
//     } catch (_err) {
//       //why cant in just get the error from the _err type??? error types are not exposed :/
//       expect(new String(_err)).to.contain("6003")
//     }
//   });


//   it("Cant move out stuff from backpack that doesnt exist", async () => {


//     try {

//       await hero.changeBackpack(settlement.entityPda,
//         settlement.settlementComponent.programId, { water: 0, food: -1, wood: 0, stone: 0 });

//       assert(false, "should've failed but didn't ")
//     } catch (_err) {
//       //why cant in just get the error from the _err type??? error types are not exposed :/
//       expect(new String(_err)).to.contain("6001")
//     }
//   });
// });

