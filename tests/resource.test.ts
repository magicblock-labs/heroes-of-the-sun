import { assert, expect } from "chai";
import { BuildingType, SettlementWrapper } from "./wrappers/settlement.wrapper";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { TokenWrapper } from "./wrappers/token.wrapper";


describe("Test suite for: resource collection", () => {

    const settlement = new SettlementWrapper();
    const world = new WorldWrapper();
    const token = new TokenWrapper();

    it("Initializes (with a default town hall) if needed", async () => {
        await settlement.init(await world.getWorldPda(), 5, 0);
        const state = await settlement.reset();
        expect(state.buildings.length).to.eq(1);
    });


    it("Builds a WaterStorage", async () => {
        let stateBefore = await settlement.build({ x: 10, y: 1, config_index: BuildingType.WaterStorage, worker_index: 0 });
        const state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) }, token.getMintExtraAccounts());
        expect(state.buildings[1].turnsToBuild).to.eq(0)
    });


    it("Upgrades Townhall", async () => {

        const stateBefore = await settlement.state();
        await settlement.upgradeAndWait(0, 0, token.getMintExtraAccounts());

        const state = await settlement.state();
        expect(state.buildings[0].level).to.eq(stateBefore.buildings[0].level + 1)
    });


    it("Upgrades a WaterStorage", async () => {
        const stateBefore = await settlement.state();

        await settlement.upgradeAndWait(1, 0, token.getMintExtraAccounts());

        const state = await settlement.state();
        expect(state.buildings[1].level).to.eq(stateBefore.buildings[1].level + 1)
    });

    it("Builds a WaterCollector", async () => {
        let stateBefore = (await settlement.state());
        const state = await settlement.build({ x: 1, y: 1, config_index: BuildingType.WaterCollector, worker_index: 0 });
        expect(state.buildings.length).to.gt(1);
        expect(state.treasury.wood).to.lt(stateBefore.treasury.wood)
    });

    it("Doesn't collect water before building is complete", async () => {
        let stateBefore = (await settlement.state());
        const state = await settlement.wait({ time: 1 }, token.getMintExtraAccounts());
        expect(state.treasury.water).to.lt(stateBefore.treasury.water)
    });

    it("Collects at least as much water as consumed, after building is complete", async () => {
        let stateBefore = (await settlement.state());
        stateBefore = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) }, token.getMintExtraAccounts());
        const state = await settlement.wait({ time: 1 }, token.getMintExtraAccounts());
        expect(state.treasury.water).to.gte(stateBefore.treasury.water)
    });

    it("Keeps collecting water even without a worker assigned (while building food storage)", async () => {
        const stateBefore = await settlement.build({ x: 1, y: 10, config_index: BuildingType.FoodStorage, worker_index: 0 });
        const state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) }, token.getMintExtraAccounts());
        expect(state.treasury.water).to.gte(stateBefore.treasury.water)
    });

    it("Builds FoodStorage", async () => {
        const stateBefore = await settlement.build({ x: 16, y: 16, config_index: BuildingType.FoodStorage, worker_index: 0 });
        const state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) }, token.getMintExtraAccounts());
        expect(state.buildings[state.buildings.length - 1].turnsToBuild).to.eq(0)
    });


    it("Upgrades a FoodStorage", async () => {
        const stateBefore = await settlement.state();
        const idx = stateBefore.buildings.length - 1;
        const state = await settlement.upgradeAndWait(idx, 0, token.getMintExtraAccounts());

        expect(state.buildings[idx].level).to.eq(stateBefore.buildings[idx].level + 1)
    });

    it("Builds FoodCollector", async () => {
        const stateBefore = await settlement.build({ x: 12, y: 12, config_index: BuildingType.FoodCollector, worker_index: 0 });
        const state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) }, token.getMintExtraAccounts());
        expect(state.buildings[state.buildings.length - 1].turnsToBuild).to.eq(0)
    });


    it("Collects as much food as being consumed", async () => {
        const stateBefore = await settlement.state();
        console.log(stateBefore);
        const state = await settlement.wait({ time: 1 }, token.getMintExtraAccounts());
        expect(state.treasury.food).to.gte(stateBefore.treasury.food)
    });


    after(async () => {
        console.log(await settlement.state());
    })
});

