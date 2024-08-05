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

  it("Doesn't collect without labour assigned", async () => {
    var waterBefore = (await settlement.state()).treasury.water;
    const state = await settlement.wait({ days: 1 });
    expect(state.treasury.water).to.eq(waterBefore);
  });


  it("Allocates Labour to Water Collector", async () => {
    const state = await settlement.assignLabour({ labour_index: 0, building_index: 1 });
    expect(state.labourAllocation[0]).to.eq(1);
  });


  it("Collect water from environment", async () => {
    var waterBefore = (await settlement.state()).treasury.water;
    const state = await settlement.wait({ days: 1 });
    expect(state.treasury.water).to.gt(waterBefore);
  });

  it("Builds a wood collector", async () => {
    const state = await settlement.build({ x: 1, y: 3, config_index: 3 });
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
    const state = await settlement.assignLabour({ labour_index: 1, building_index: 2 });
    expect(state.labourAllocation[0]).to.eq(1);
  });


  it("Collect wood from environment", async () => {
    var woodBefore = (await settlement.state()).treasury.wood;
    const state = await settlement.wait({ days: 1 });
    expect(state.treasury.wood).to.gt(woodBefore);
  });

  it("Time Deteriorates buildings", async () => {
    var deteriorationBefore = (await settlement.state()).buildings[0].deterioration;
    const state = await settlement.wait({ days: 5 });
    expect(state.buildings[0].deterioration).to.eq(deteriorationBefore + 5);
  });

  after(async () => {
    console.log(await settlement.state());
  })
});
