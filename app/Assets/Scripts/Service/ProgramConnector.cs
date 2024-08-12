using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Settlement;
using Settlement.Program;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Unity.VisualScripting;
using UnityEngine;
using World.Program;

namespace Service
{
    [Singleton]
    public class ProgramConnector
    {
        private SettlementClient _settlement;

        private SettlementClient Settlement => _settlement ?? (_settlement = new(Web3.Rpc, Web3.WsRpc, new PublicKey(SettlementProgram.ID)));

        public async Task ResetAccounts()
        {
            throw new NotImplementedException();
        }

        public async Task Initialise()
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");

            var walletBase = Web3.Wallet;
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    WorldProgram.InitializeNewWorld(new InitializeNewWorldAccounts
                    {
                        Payer = Web3.Account.PublicKey,
                        World = new (WorldProgram.ID),
                        Registry = new PublicKey("EHLkWwAT9oebVv9ht3mtqrvHhRVMKrt54tF3MfHTey2K"),
                        SystemProgram = SystemProgram.ProgramIdKey
                    })
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };
            var result = await walletBase.SignAndSendTransaction(tx);
            
            Debug.Log(result);

        }

        public async Task<bool> ReloadData()
        {
            
            var rawData = await Settlement.GetSettlementAsync(Web3.Account.PublicKey);
        
            if (rawData.ParsedResult == null)
                return false;
        
            return true;
        }

        public async Task<bool> PlaceBuilding(byte x, byte z, byte type)
        {
            throw new System.NotImplementedException();
        }
    }
}