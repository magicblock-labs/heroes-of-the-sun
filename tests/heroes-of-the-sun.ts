import { assert, expect } from "chai";
import { SettlementWrapper } from "./settlement.wrapper";

describe("HeroesOfTheSun", () => {

  const settlement = new SettlementWrapper();

  it("Initializes", async () => {
    const state = await settlement.init();
    expect(state.buildings.length).to.eq(0);
  });

  it("Adds a building", async () => {
    const state = await settlement.build({ x: 1, y: 1, id: 1 });
    expect(state.buildings.length).to.gt(0);
  });

  it("Prevents overlap", async () => {
    try {
      await settlement.build({ x: 1, y: 1, id: 1 });
      assert(false, "should've failed but didn't ")
    } catch (_err) {
      //why cant in just get the error from the _err type??? error types are not exposed :/
      expect(new String(_err)).to.contain("6000")
    }
  });

  it("Allocates Labour", async () => {
    const state = await settlement.assignLabour({ labour: 0, building: 0 });
    console.log(state);
  });

  it("Waits a day", async () => {
    // Check that the system has been applied and x is > 0
    const state = await settlement.wait({ days: 5 });
    console.log(state);
  });

});
