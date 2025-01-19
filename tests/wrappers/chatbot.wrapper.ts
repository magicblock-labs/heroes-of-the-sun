import * as anchor from "@coral-xyz/anchor";


import { DeityBot } from "../../target/types/deity_bot";
import { BN, web3 } from "@coral-xyz/anchor";

export class ChatbotWrapper {
  provider: anchor.AnchorProvider;
  program: anchor.Program<DeityBot>;

  constructor() {

    this.provider = anchor.AnchorProvider.env();
    anchor.setProvider(this.provider);
    this.program = anchor.workspace.DeityBot as anchor.Program<DeityBot>;
  }

  async initialize() {


    const agent = web3.PublicKey.findProgramAddressSync(
      [Buffer.from("agent")],
      this.program.programId
    )[0];

    var llm = "LLMrieZMpbJFwN52WgmBNMxYojrpRVYXdC1RCweEbab";

    const counterAddress = web3.PublicKey.findProgramAddressSync(
      [Buffer.from("counter")],
      new anchor.web3.PublicKey(llm)
    )[0];

    const counter = await this.provider.connection.getAccountInfo(counterAddress);

    // convert buffer to bn (looks like i'm missing something lol)
    const counterByte = counter.data.readBigInt64LE(8);
    const contextAddress = web3.PublicKey.findProgramAddressSync(
      [
        Buffer.from("context"),
        new BN(counterByte.toString()).toArrayLike(Buffer, "le", 4),
      ],
      new anchor.web3.PublicKey(llm)
    )[0];


    console.log(counterByte);

    const transactionSignature = await this.program.methods
      .initialiseAgent()
      .accounts({
        payer: this.provider.wallet.publicKey,
        counter: counterAddress,
        agent,
        llmContext: contextAddress,
      })
      .rpc();

    console.log("Success!");
    console.log(`Transaction Signature: ${transactionSignature} `);

  }
};
