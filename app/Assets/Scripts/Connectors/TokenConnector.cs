using System;
using System.Threading.Tasks;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;


namespace Connectors
{
    struct TokenBalance
    {
        public int uiAmount;
    }
    
    [Singleton]
    public class TokenConnector
    {
        private const string TokenMintPda = "Fn7ndp5EocCfzDkFMdWUZj5B55AoM7nA5o5cXSUbtDrn";

        private string _ata;

        private string AssociatedTokenAccount => _ata ??=
            AssociatedTokenAccountProgram
                .DeriveAssociatedTokenAccount(Web3.Account, new PublicKey(TokenMintPda));

        public async Task Subscribe(Action<SubscriptionState, ResponseValue<AccountInfo>, AccountInfo> callback)
        {
            await Web3.Wallet.ActiveStreamingRpcClient.SubscribeAccountInfoAsync(AssociatedTokenAccount, (s, e) =>
            {
                Debug.Log(e.Value.Data[0]);
            }, Commitment.Processed);
        }
    }
}