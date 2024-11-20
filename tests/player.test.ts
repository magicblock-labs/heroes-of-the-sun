import { expect } from "chai";
import { WorldWrapper } from "./wrappers/world.wrapper";
import { PlayerWrapper } from "./wrappers/player.wrapper";
import { SettlementWrapper } from "./wrappers/settlement.wrapper";
import { LocationAllocatorWrapper } from "./wrappers/location_allocator.wrapper";

import * as anchor from "@coral-xyz/anchor";
import { TokenMinter } from "../target/types/token_minter";
import { PublicKey } from "@solana/web3.js";
import { getAssociatedTokenAddressSync } from "@solana/spl-token";


describe("Creates A Player And Assigns a Settlement", async () => {

  const player = new PlayerWrapper();
  const world = new WorldWrapper();
  const settlement = new SettlementWrapper();
  const locationAllocator = new LocationAllocatorWrapper();


  ///todo TOKEN WRAPPER

  const program = anchor.workspace.TokenMinter as anchor.Program<TokenMinter>;
  // Derive the PDA to use as mint account address.
  // This same PDA is also used as the mint authority.
  const [mintPDA] = PublicKey.findProgramAddressSync(
    [Buffer.from("mint")],
    program.programId
  );

  const metadata = {
    name: "Magical Gem",
    symbol: "MBGEM",
    uri: "https://shdw-drive.genesysgo.net/4PMP1MG5vYGkT7gnAMb7E5kqPLLjjDzTiAaZ3xRx5Czd/gem.json",
  };


  const provider = anchor.AnchorProvider.env();
  anchor.setProvider(provider);

  it("creates a token", async () => {
    const transactionSignature = await program.methods
      .createToken(metadata.name, metadata.symbol, metadata.uri)
      .accounts({
        payer: provider.wallet.publicKey,
      })
      .rpc();

    console.log("Success!");
    console.log(`   Mint Address: ${mintPDA}`);
    console.log(`   Transaction Signature: ${transactionSignature}`);
  });

  ///todo TOKEN WRAPPER ====================

  it("Initializes a player", async () => {
    await player.init(await world.getWorldPda());
    await locationAllocator.init(await world.getWorldPda())

    const state = await player.state();
    expect(state.settlements.length).to.eq(0);
  });



  it("Mint 1 Token for player", async () => {
    // Derive the associated token address account for the mint and payer.
    const associatedTokenAccountAddress = getAssociatedTokenAddressSync(
      mintPDA,
      provider.wallet.publicKey
    );

    // Amount of tokens to mint.
    const amount = new anchor.BN(1);

    const transactionSignature = await program.methods
      .mintToken(amount)
      .accounts({
        payer: provider.wallet.publicKey,
        associatedTokenAccount: associatedTokenAccountAddress,
        playerData: player.entityPda,
      })
      .rpc();

    console.log("Success!");
    console.log(
      `   Associated Token Account Address: ${associatedTokenAccountAddress}`
    );
    console.log(`   Transaction Signature: ${transactionSignature}`);
  });

  it("Assigns settlement to a player", async () => {
    const allocatorState = (await locationAllocator.state());
    console.log("!", allocatorState);
    await settlement.init(await world.getWorldPda(), allocatorState.currentX, allocatorState.currentY)
    console.log("!", await settlement.state());
    const state = await player.assignSettlement(
      settlement.entityPda,
      settlement.settlementComponent.programId,
      locationAllocator.entityPda,
      locationAllocator.locationAllocatorComponent.programId,
    );
    expect(state.settlements.length).to.gt(0);
  });


  after(async () => {
    console.log(await player.state());
    console.log(await settlement.state());
    console.log(await locationAllocator.state());
  })
});

