import * as anchor from "@coral-xyz/anchor";
import { PublicKey } from "@solana/web3.js";
import {
  InitializeNewWorld,
} from "@magicblock-labs/bolt-sdk"


export class WorldWrapper {

  provider: anchor.AnchorProvider;
  static worldPda: PublicKey;

  async getWorldPda() {
    if (!WorldWrapper.worldPda) {
      this.provider = anchor.AnchorProvider.env();
      anchor.setProvider(this.provider);

      const initNewWorld = await InitializeNewWorld({
        payer: this.provider.wallet.publicKey,
        connection: this.provider.connection,
      });
      let txSign = await this.provider.sendAndConfirm(initNewWorld.transaction);
      WorldWrapper.worldPda = initNewWorld.worldPda;

      console.log(`Initialized a new world \x1b[31m (PDA = ${WorldWrapper.worldPda}, ID = ${initNewWorld.worldId}\x1b[0m).`);
      console.log(`Initialization signature: ${txSign}`);
    }

    return WorldWrapper.worldPda;
  }
};
