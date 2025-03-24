using System;
using System.Threading.Tasks;
using GplSession.Accounts;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using UnityEngine;

namespace Utils
{
    public static class Web3Utils
    {
        public static SessionToken SessionToken;

        public static readonly InGameWallet EphemeralWallet = new(RpcCluster.DevNet, "https://devnet.magicblock.app", "wss://devnet.magicblock.app", true);
        
        public static SessionWallet SessionWallet;
        public static long SessionValidUntil;
        
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