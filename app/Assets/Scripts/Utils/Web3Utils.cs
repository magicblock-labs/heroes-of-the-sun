using System;
using System.Linq;
using System.Threading.Tasks;
using GplSession.Accounts;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using World.Program;
using Random = UnityEngine.Random;

namespace Utils
{
    public static class Web3Utils
    {
        public static SessionToken SessionToken;

        public static readonly InGameWallet EphemeralWallet = new(RpcCluster.DevNet, "https://devnet.magicblock.app", "wss://devnet.magicblock.app", true);
        
        public static SessionWallet SessionWallet;
        public static long SessionValidUntil;
        
        private static long _timeOffset;
        
        private const string SessionPwdPrefKey = nameof(SessionPwdPrefKey);


        public static async Task RefreshSessionWallet()
        {
            var password = PlayerPrefs.GetString(SessionPwdPrefKey, null);

            if (string.IsNullOrEmpty(password))
            {
                password = RandomString(10);
                PlayerPrefs.SetString(SessionPwdPrefKey, password);
            }

            Web3Utils.SessionWallet = await SessionWallet.GetSessionWallet(new PublicKey(WorldProgram.ID),
                password,
                Web3.Wallet);
        }

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


        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Range(0, s.Length)]).ToArray());
        }
    }
}