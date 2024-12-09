import { expect } from "chai";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { PlayerWrapper } from "./wrappers/player.wrapper";
import { HeroWrapper } from "./wrappers/hero.wrapper";
import { SmartObjectWrapper } from "./wrappers/smart_object.wrapper";



describe("Smart objects tests", async () => {

  const player = new PlayerWrapper();
  const hero = new HeroWrapper();
  const world = new WorldWrapper();
  const smartObject = new SmartObjectWrapper();


  it("Initializes a player", async () => {
    await player.init(await world.getWorldPda());

    const state = await player.state();
    expect(state.settlements.length).to.eq(0);
  });

  it("Assigns hero to a player", async () => {
    await hero.init(await world.getWorldPda())
    await player.assignHero(
      hero.entityPda,
      hero.heroComponent.programId,
    );
    const state = await hero.state();
    expect(state.owner.toString()).to.not.be.null;
  });


  it("Creates a smart obejct", async () => {
    await smartObject.init(await world.getWorldPda())

    var bytesArray = [];
    for (var byte of smartObject.entityPda.toBytes())
      bytesArray.push(byte)


    await smartObject.initObj({ x: -1, y: 0, entity: bytesArray })
    const location = await smartObject.location();
    expect(location.x).to.eq(-1);
    expect(location.entity.toBase58()).to.eq(smartObject.entityPda.toBase58());
    const deity = await smartObject.deity();
    expect(deity.system).to.be.not.null;
  });

  it("Moves hero to a pos", async () => {
    const state = await hero.moveHero({ x: 1, y: 1 });
    expect(state.x).to.eq(1);
    expect(state.y).to.eq(1);
  });

  it("Interacts with deity", async () => {

    await smartObject.interact({ index: 1 }, hero.entityPda,
      hero.heroComponent.programId,);
    const deity = await smartObject.deity();
    expect(deity.state).gt(0);
  });

});

