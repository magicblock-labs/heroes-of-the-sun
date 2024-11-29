import * as anchor from "@coral-xyz/anchor";
import { AccountMeta, PublicKey } from "@solana/web3.js";
import {
  InitializeNewWorld,
} from "@magicblock-labs/bolt-sdk"


import { TokenMinter } from "../../target/types/token_minter";
import { ASSOCIATED_TOKEN_PROGRAM_ID, getAssociatedTokenAddressSync, TOKEN_PROGRAM_ID } from "@solana/spl-token";
import { SYSTEM_PROGRAM_ID } from "@coral-xyz/anchor/dist/cjs/native/system";

export class TokenWrapper {
  mintPDA: anchor.web3.PublicKey;
  metadata: { name: string; symbol: string; uri: string; };
  associatedTokenAccountAddress: anchor.web3.PublicKey;
  provider: anchor.AnchorProvider;
  program: anchor.Program<TokenMinter>;

  constructor() {


    this.program = anchor.workspace.TokenMinter as anchor.Program<TokenMinter>;
    // Derive the PDA to use as mint account address.
    // This same PDA is also used as the mint authority.
    this.mintPDA = PublicKey.findProgramAddressSync(
      [Buffer.from("mint")],
      this.program.programId
    )[0];

    this.metadata = {
      name: "Magical Gem",
      symbol: "MBGEM",
      uri: "https://shdw-drive.genesysgo.net/4PMP1MG5vYGkT7gnAMb7E5kqPLLjjDzTiAaZ3xRx5Czd/gem.json",
    };


    this.provider = anchor.AnchorProvider.env();
    anchor.setProvider(this.provider);

    console.log(`SPL Token: \x1b[31m (mintPDA = ${this.mintPDA} \x1b[0m).`);

    // Derive the associated token address account for the mint and payer.
    this.associatedTokenAccountAddress = getAssociatedTokenAddressSync(
      this.mintPDA,
      this.provider.wallet.publicKey
    );


  }

  async createToken() {
    const transactionSignature = await this.program.methods
      .createToken(this.metadata.name, this.metadata.symbol, this.metadata.uri)
      .accounts({
        payer: this.provider.wallet.publicKey,
      })
      .rpc();

    console.log("Success!");
    console.log(`Mint Address: ${this.mintPDA} `);
    console.log(`Transaction Signature: ${transactionSignature} `);

  }

  async getBalance() {
    return await this.provider.connection.getTokenAccountBalance(this.associatedTokenAccountAddress)
  }



  getMintExtraAccounts(): AccountMeta[] {
    return [
      {
        pubkey: this.associatedTokenAccountAddress,
        isSigner: false,
        isWritable: true
      },
      {
        pubkey: this.mintPDA,
        isSigner: false,
        isWritable: true
      },
      {
        pubkey: this.program.programId,
        isSigner: false,
        isWritable: false
      },
      {
        pubkey: TOKEN_PROGRAM_ID,
        isSigner: false,
        isWritable: false
      },
      {
        pubkey: ASSOCIATED_TOKEN_PROGRAM_ID,
        isSigner: false,
        isWritable: false
      },
      {
        pubkey: SYSTEM_PROGRAM_ID,
        isSigner: false,
        isWritable: false
      }
    ];
  }

  getBurnExtraAccounts(): AccountMeta[] {
    return [
      {
        pubkey: this.associatedTokenAccountAddress,
        isSigner: false,
        isWritable: true
      },
      {
        pubkey: this.mintPDA,
        isSigner: false,
        isWritable: true
      },
      {
        pubkey: this.program.programId,
        isSigner: false,
        isWritable: false
      },
      {
        pubkey: TOKEN_PROGRAM_ID,
        isSigner: false,
        isWritable: false
      },
      {
        pubkey: ASSOCIATED_TOKEN_PROGRAM_ID,
        isSigner: false,
        isWritable: false
      }
    ]
  }
};
