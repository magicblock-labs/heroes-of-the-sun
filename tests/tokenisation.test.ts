import { assert, expect } from "chai";
import { BuildingType, SettlementWrapper } from "./wrappers/settlement.wrapper";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { TokenWrapper } from "./wrappers/token.wrapper";
import { LootDistributionWrapper } from "./wrappers/loot_distribution.wrapper";
import { HeroWrapper } from "./wrappers/hero.wrapper";
import { PlayerWrapper } from "./wrappers/player.wrapper";
import { LocationAllocatorWrapper } from "./wrappers/location_allocator.wrapper";


describe("Test suite for: tokenized gold source and sink", () => {

    const settlement = new SettlementWrapper();
    const token = new TokenWrapper();
    const world = new WorldWrapper();
    const lootDistribution = new LootDistributionWrapper();
    const hero = new HeroWrapper();
    const player = new PlayerWrapper();
    const locationAllocator = new LocationAllocatorWrapper();


    // it("creates a token", async () => {
    //     await token.createToken();//
    // });

    // it("Initializes (with a default town hall) if needed", async () => {
    //     await settlement.init(await world.getWorldPda(), 5, 2);
    //     const state = await settlement.reset();
    //     expect(state.buildings.length).to.eq(1);
    // });


    it("Inits loot distribution", async () => {

        await lootDistribution.init(await world.getWorldPda());
        const lootState = await lootDistribution.state();
        expect(lootState.loots.length).gt(0);
    });

    // it("Initializes a player", async () => {
    //     await player.init(await world.getWorldPda());
    //     await locationAllocator.init(await world.getWorldPda())

    //     const state = await player.state();
    //     expect(state.settlements.length).to.eq(0);
    // });

    // it("Assigns hero to a player", async () => {
    //     await hero.init(await world.getWorldPda())
    //     await player.assignHero(
    //         hero.entityPda,
    //         hero.heroComponent.programId,
    //     );
    //     const state = await hero.state();
    //     expect(state.owner.toString()).to.not.be.null;
    // });

    // it("Collects Loot", async () => {

    //     await lootDistribution.init(await world.getWorldPda());
    //     const lootState = await lootDistribution.state();
    //     await hero.moveHero(lootState.loots[0]);

    //     await lootDistribution.claimLoot({ index: 0 }, token.getMintExtraAccounts());

    //     const balanceAfter = await token.getBalance();
    //     expect(balanceAfter.value.uiAmount).to.gt(0);
    // });

    // it("Exchanges toekn for food", async () => {

    //     const settlementBefore = await settlement.state()
    //     const balanceBefore = await token.getBalance();

    //     const settlementAfter = await settlement.exchange({ tokens_for_wood: 1, tokens_for_food: 0, tokens_for_water: 0, tokens_for_stone: 0 }, token.getBurnExtraAccounts());
    //     const balanceAfter = await token.getBalance();

    //     expect(balanceAfter.value.uiAmount).to.lt(balanceBefore.value.uiAmount);
    //     expect(settlementAfter.treasury.wood).to.gt(settlementBefore.treasury.wood);
    // });


    // it("Fails to exchange token if insufficient", async () => {
    //     try {
    //         await settlement.exchange({ tokens_for_wood: 1, tokens_for_food: 0, tokens_for_water: 0, tokens_for_stone: 0 }, token.getBurnExtraAccounts());
    //         assert(false, "should've failed but didn't ")
    //     } catch (_err) {
    //         //why cant in just get the error from the _err type??? error types are not exposed :/
    //         expect(new String(_err)).to.contain("insufficient funds")
    //     }
    // });
});
