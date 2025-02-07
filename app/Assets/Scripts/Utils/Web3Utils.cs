using System;
using System.Threading.Tasks;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using UnityEngine;

namespace Utils
{
    public static class Web3Utils
    {
        
        public static readonly InGameWallet EphemeralWallet = new(RpcCluster.DevNet, "https://devnet.magicblock.app", "wss://devnet.magicblock.app", true);
        public static SessionWallet SessionWallet { get; set; }

        public static async Task EnsureBalance()
        {
            var requestResult = await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey);
            Debug.Log($"{Web3.Account.PublicKey} {requestResult.Result.Value} ");

            if (requestResult.Result.Value < 50000000)
            {
                await Airdrop();
            }
        }

        public static async Task Airdrop()
        {
            var airdropResult = await Web3.Rpc.RequestAirdropAsync(Web3.Account.PublicKey, 100000000);
            var txResult = await Web3.Rpc.ConfirmTransaction(airdropResult.Result, Commitment.Confirmed);
            var balanceResult = await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey);
            Debug.Log(
                $"{Web3.Account.PublicKey} \nairdropResult.Result: {airdropResult.Result}, \ntxResult{txResult} \n balanceResult:{balanceResult} ");
        }
        
        private static long _timeOffset;

        public static async Task SyncTime()
        {
            var slot = await Web3.Rpc.GetSlotAsync(Commitment.Processed);
            var nodeTimestamp = await Web3.Rpc.GetBlockTimeAsync(slot.Result);
            _timeOffset = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (long)nodeTimestamp.Result;

            Debug.Log(_timeOffset);
        }

        public static long GetNodeTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _timeOffset;
        }
    }
}