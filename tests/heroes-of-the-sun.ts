import { assert, expect } from "chai";
import { SettlementWrapper } from "./settlement.wrapper";

describe("HeroesOfTheSun", () => {

  const settlement = new SettlementWrapper();

  it("Initializes (with a default town hall", async () => {
    const state = await settlement.init();
    expect(state.buildings.length).to.eq(1);
  });

  it("Builds a water collector", async () => {
    const state = await settlement.build({ x: 1, y: 1, config_index: 1 });
    expect(state.buildings.length).to.gt(1);
  });

  it("Prevents building overlap", async () => {
    try {
      await settlement.build({ x: 1, y: 1, config_index: 1 });
      assert(false, "should've failed but didn't ")
    } catch (_err) {
      //why cant in just get the error from the _err type??? error types are not exposed :/
      expect(new String(_err)).to.contain("6000")
    }
  });

  it("Cant collect resource without storage", async () => {
    var waterBefore = (await settlement.state()).treasury.water;
    const state = await settlement.wait({ time: 1 });
    expect(state.treasury.water).to.lte(waterBefore);
  });

  it("Builds a water storage", async () => {
    var lengthBefore = (await settlement.state()).buildings.length;
    const state = await settlement.build({ x: 1, y: 6, config_index: 4 });
    expect(state.buildings.length).to.gt(lengthBefore);
  });

  it("Upgrades water storage", async () => {
    const state = await settlement.upgrade({ index: 2 });
    expect(state.buildings.length).to.gt(1);
  });

  it("Collect water from environment", async () => {
    var waterBefore = (await settlement.state()).treasury.water;
    const state = await settlement.wait({ time: 1 });
    console.log(state);
    expect(state.treasury.water).to.gt(waterBefore);
  });

  it("Builds a food collector", async () => {
    const state = await settlement.build({ x: 1, y: 3, config_index: 2 });
    expect(state.buildings.length).to.gt(1);
  });

  it("Prevents allocation of labour beyond townhall level", async () => {
    try {
      await settlement.assignLabour({ labour_index: 1, building_index: 2 });
      assert(false, "should've failed but didn't ")
    } catch (_err) {
      //why cant in just get the error from the _err type??? error types are not exposed :/
      expect(new String(_err)).to.contain("6000")
    }
  });

  it("Upgrades townhall", async () => {
    const state = await settlement.upgrade({ index: 0 });
    expect(state.buildings.length).to.gt(1);
  });

  it("Allocates Labour To Food Collector", async () => {
    const state = await settlement.assignLabour({ labour_index: 1, building_index: 3 });
    expect(state.labourAllocation[1]).to.eq(3);
  });

  it("Cant collect food (different resource) from environment without storage", async () => {
    var foodBefore = (await settlement.state()).treasury.food;
    const state = await settlement.wait({ time: 1 });
    expect(state.treasury.food).to.lte(foodBefore);
  });

  it("Builds a food storage", async () => {
    var lengthBefore = (await settlement.state()).buildings.length;
    const state = await settlement.build({ x: 1, y: 10, config_index: 5 });
    expect(state.buildings.length).to.gt(lengthBefore);
  });

  it("Collects food from environment", async () => {
    var foodBefore = (await settlement.state()).treasury.food;
    const state = await settlement.wait({ time: 1 });
    expect(state.treasury.food).to.gte(foodBefore); //gte here cause we ahve 2 labour that eats everything collected
  });

  it("Time Deteriorates buildings", async () => {
    var deteriorationBefore = (await settlement.state()).buildings[0].deterioration;
    const state = await settlement.wait({ time: 5 });
    expect(state.buildings[0].deterioration).to.eq(deteriorationBefore + 5);
  });

  after(async () => {
    console.log(await settlement.state());
  })
});
