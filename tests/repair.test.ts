import { assert, expect } from "chai";
import { BuildingType, SettlementWrapper } from "./wrappers/settlement.wrapper";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { TokenWrapper } from "./wrappers/token.wrapper";


describe("Test suite for: deterioration repair", () => {

    const settlement = new SettlementWrapper(); const token = new TokenWrapper();
    const world = new WorldWrapper();

    it("Initializes (with a default town hall) if needed", async () => {
        await settlement.init(await world.getWorldPda(), 4, 0);
        const state = await settlement.reset();
        expect(state.buildings.length).to.eq(1);
    });

    it("Builds a WaterCollector", async () => {
        let state = await settlement.build({ x: 1, y: 10, config_index: BuildingType.WaterCollector, worker_index: 0 });
        state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(state) }, token.getMintExtraAccounts())
        expect(state.buildings[1].turnsToBuild).to.eq(0);
    });

    it("Breaks over time", async () => {
        let state = await settlement.wait({ time: 50 }, token.getMintExtraAccounts())
        expect(state.buildings[1].deterioration).to.eq(50);
    });


    it("Repairs", async () => {
        var stateBefore = (await settlement.state());
        let state = await settlement.repair({ index: 1 })
        expect(state.buildings[1].deterioration).to.eq(0);
        expect(stateBefore.treasury.wood).to.gt(state.treasury.wood);
    });

});
