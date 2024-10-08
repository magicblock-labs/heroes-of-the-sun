import { assert, expect } from "chai";
import { BuildingType, SettlementWrapper } from "./settlement.wrapper";


describe("Test suite for: resource collection", () => {

  const settlement = new SettlementWrapper();

  it("Initializes (with a default town hall) if needed", async () => {
    const state = await settlement.reset();
    expect(state.buildings.length).to.eq(1);
  });


  it("Builds a WaterStorage", async () => {
    let stateBefore = await settlement.build({ x: 10, y: 1, config_index: BuildingType.WaterStorage, worker_index: 0 });
    const state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) });
    expect(state.buildings[1].turnsToBuild).to.eq(0)
  });


  it("Upgrades a WaterStorage", async () => {
    let stateBefore = (await settlement.state());
    await settlement.upgrade({ index: 1 });
    const state = await settlement.upgrade({ index: 1 });
    expect(state.buildings[1].level).to.gt(stateBefore.buildings[1].level)
  });

  it("Builds a WaterCollector", async () => {
    let stateBefore = (await settlement.state());
    const state = await settlement.build({ x: 1, y: 1, config_index: BuildingType.WaterCollector, worker_index: 0 });
    expect(state.buildings.length).to.gt(1);
    expect(state.treasury.wood).to.lt(stateBefore.treasury.wood)
  });

  it("Doesn't collect water before building is complete", async () => {
    let stateBefore = (await settlement.state());
    const state = await settlement.wait({ time: 1 });
    expect(state.treasury.water).to.lt(stateBefore.treasury.water)
  });

  it("Collects more water then consumed, after building is complete", async () => {
    let stateBefore = (await settlement.state());
    stateBefore = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) });
    const state = await settlement.wait({ time: 1 });
    expect(state.treasury.water).to.gt(stateBefore.treasury.water)
  });

  it("Keeps collecting water even without a worker assigned (while building food storage)", async () => {
    const stateBefore = await settlement.build({ x: 1, y: 10, config_index: BuildingType.FoodStorage, worker_index: 0 });
    const state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) });
    expect(state.treasury.water).to.gt(stateBefore.treasury.water)
  });


  it("Builds FoodCollector", async () => {
    const stateBefore = await settlement.build({ x: 12, y: 12, config_index: BuildingType.FoodCollector, worker_index: 0 });
    const state = await settlement.wait({ time: settlement.getTurnsToCompleteAll(stateBefore) + 1 });
    expect(state.treasury.food).to.gt(stateBefore.treasury.food)
  });


  after(async () => {
    console.log(await settlement.state());
  })
});

