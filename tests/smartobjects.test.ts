import { expect } from "chai";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { PlayerWrapper } from "./wrappers/player.wrapper";
import { HeroWrapper } from "./wrappers/hero.wrapper";
import { SmartObjectWrapper } from "./wrappers/smart_object.wrapper";
import { Account, Keypair, PublicKey } from "@solana/web3.js";
import { TokenWrapper } from "./wrappers/token.wrapper";



describe("Smart objects tests", async () => {

    const player = new PlayerWrapper();
    const hero = new HeroWrapper();
    const world = new WorldWrapper();
    const smartObject = new SmartObjectWrapper();
    const token = new TokenWrapper();


    // it("Initializes a player", async () => {
    //     await player.init(await world.getWorldPda());

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


    // it("Creates a world", async () => {
    // await world.getWorldPda()

    // await smartObject.init(await world.getWorldPda())

    // var bytesArray = [];
    // for (var byte of smartObject.entityPda.toBytes())
    //     bytesArray.push(byte)

    // await smartObject.initObj({ x: -1, y: 0, entity: bytesArray })
    // const location = await smartObject.location();
    // expect(location.x).to.eq(-1);
    // expect(location.entity.toBase58()).to.eq(smartObject.entityPda.toBase58());

    // await smartObject.createTokenLauncherComponent();
    // let tokenLauncher = await smartObject.tokenLauncher();
    // expect(tokenLauncher.system).to.be.not.null;


    // await smartObject.initTokenLauncher({
    //     token_name: "test",
    //     token_symbol: "TST",
    //     token_uri: "https://gateway.pinata.cloud/ipfs/bafkreifv442zndondi3faqb4ebgiptyip2etqh2ipik4sh2yikfim2en7q",
    //     recipe_food: 3,
    //     recipe_stone: 3,
    //     recipe_water: 1,
    //     recipe_wood: 2
    // })


    // console.log(tokenLauncher)
    // });


});

