import { expect } from "chai";
import { BuildingType, SettlementWrapper } from "./wrappers/settlement.wrapper";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { TokenWrapper } from "./wrappers/token.wrapper";


describe("Test suite for: tokenized gold source and sink", () => {

    const settlement = new SettlementWrapper();
    const token = new TokenWrapper();
    const world = new WorldWrapper();


    it("creates a token", async () => {
        await token.createToken();//
    });

    it("Initializes (with a default town hall) if needed", async () => {
        await settlement.init(await world.getWorldPda(), 5, 2);
        const state = await settlement.reset();
        expect(state.buildings.length).to.eq(1);
    });

    it("Builds a GoldCollector", async () => {
        let state = await settlement.build({ x: 1, y: 10, config_index: BuildingType.GoldCollector, worker_index: 0 });
        state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(state) }, token.getMintExtraAccounts())
        expect(state.buildings[1].turnsToBuild).to.eq(0);
    });

    it("Collects Gold", async () => {
        const balanceBefore = await token.getBalance();
        await settlement.wait({ time: 5 }, token.getMintExtraAccounts())
        const balanceAfter = await token.getBalance();
        expect(balanceAfter.value.uiAmount).to.gt(balanceBefore.value.uiAmount);
    });


    it("Builds a ResearchFacility", async () => {
        let state = await settlement.build({ x: 13, y: 13, config_index: BuildingType.Research, worker_index: 0 });
        state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(state) }, token.getMintExtraAccounts())
        expect(state.buildings[2].turnsToBuild).to.eq(0);
    });

    it("Researches For Gold", async () => {
        const balanceBefore = await token.getBalance();
        await settlement.research({ research_type: 3 }, token.getBurnExtraAccounts())
        const balanceAfter = await token.getBalance();
        expect(balanceAfter.value.uiAmount).to.lt(balanceBefore.value.uiAmount);
    });

});
