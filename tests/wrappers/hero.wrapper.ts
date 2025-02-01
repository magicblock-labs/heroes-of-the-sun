import * as anchor from "@coral-xyz/anchor";
import { Program } from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  AddEntity,
  InitializeComponent,
  ApplySystem,
  FindComponentPda,
  createDelegateInstruction,
} from "@magicblock-labs/bolt-sdk"
import { Hero } from "../../target/types/hero";
import { MoveHero } from "../../target/types/move_hero";
import { ChangeBackpack } from "../../target/types/change_backpack";

export type MoveHeroArgs = {
  x: number,
  y: number
}

export type ChangeBackpackArgs = {

  food: number,
  water: number,
  wood: number,
  stone: number,
}


export class HeroWrapper {

  provider: anchor.AnchorProvider;

  worldPda: PublicKey;
  entityPda: PublicKey;
  componentPda: PublicKey;

  heroComponent: Program<Hero>;
  moveHeroSystem: Program<MoveHero>;
  changeBackpackSystem: Program<ChangeBackpack>;

  async init(worldPda: PublicKey) {

    this.worldPda = worldPda;
    if (!this.componentPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const heroEntity = await AddEntity({
        payer: this.provider.wallet.publicKey,
        world: this.worldPda,
        connection: this.provider.connection,
      });

      this.heroComponent = anchor.workspace.Hero as Program<Hero>;
      this.moveHeroSystem = anchor.workspace.MoveHero as Program<MoveHero>;
      this.changeBackpackSystem = anchor.workspace.ChangeBackpack as Program<ChangeBackpack>;

      let txSign = await this.provider.sendAndConfirm(heroEntity.transaction);
      this.entityPda = heroEntity.entityPda;
      console.log(`Initialized a new Entity (PDA=${heroEntity.entityPda}). Initialization signature: ${txSign}`);

      const initializeComponent = await InitializeComponent({
        payer: this.provider.wallet.publicKey,
        entity: this.entityPda,
        componentId: this.heroComponent.programId,
      });
      txSign = await this.provider.sendAndConfirm(initializeComponent.transaction);
      this.componentPda = initializeComponent.componentPda;
      console.log(`Initialized the hero component. Initialization signature: ${txSign}`);
    }
  }

  async state() {
    return await this.heroComponent.account.hero.fetch(this.componentPda);
  }

  async moveHero(args: MoveHeroArgs) {
    // Run the build system
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.moveHeroSystem.programId,
      entities: [{
        entity: this.entityPda,
        components: [{ componentId: this.heroComponent.programId }],
      }],
      args
    });
    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`build tx: ${txSign}`);

    return await this.state();
  }


  async delegate() {


    const counterPda = FindComponentPda({
      componentId: this.heroComponent.programId,
      entity: this.entityPda,
    });
    const delegateIx = createDelegateInstruction({
      entity: this.entityPda,
      account: this.componentPda,
      ownerProgram: this.heroComponent.programId,
      payer: this.provider.wallet.publicKey,
    });
    const tx = new anchor.web3.Transaction().add(delegateIx);
    tx.feePayer = this.provider.wallet.publicKey;
    tx.recentBlockhash = (await this.provider.connection.getLatestBlockhash()).blockhash;
    const txSign = await this.provider.sendAndConfirm(tx, [], { commitment: "confirmed" });
    console.log(
      `Delegation signature: ${txSign}`
    );


    return await this.state();
  }


  async changeBackpack(settlementPDA: PublicKey, settlementProgramID: PublicKey, args: ChangeBackpackArgs) {
    const applySystem = await ApplySystem({
      world: this.worldPda,
      authority: this.provider.wallet.publicKey,
      systemId: this.changeBackpackSystem.programId,
      entities: [
        {
          entity: this.entityPda,
          components: [{ componentId: this.heroComponent.programId }],
        },
        {
          entity: settlementPDA,
          components: [{ componentId: settlementProgramID }],
        }],
      args,
    });

    const txSign = await this.provider.sendAndConfirm(applySystem.transaction);
    console.log(`backpack tx: ${txSign}`);

    return await this.state();
  }

};
