using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Solana.Unity.Programs;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using View;
using World.Program;

namespace Connectors
{
    [Singleton]
    public abstract class BaseProgramConnector<T> : InjectableObject where T : BaseClient
    {
        //this comes from program deployment
        private const string WorldPda = "5Fj5HJud66muuDyateWdP2HAPkED7CnyApDQBMreVQQH";//"GvMv6N5UF8ctteapSXMJUh2GXmXb4a7hRHWNmi69PTA8";
        private const int WorldIndex = 2;//1318;

        protected T Client;

        private string _entityPda;
        private long _timeOffset;
        private string _dataAddress;

        private string EntityPda => _entityPda ??= Pda.FindEntityPda(WorldIndex, 0, GetExtraSeed());

        protected abstract string GetExtraSeed();
        protected abstract PublicKey GetComponentProgramAddress();
        
        public async Task EnsureBalance()
        {
            var requestResult = await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey);
            Debug.Log($"{Web3.Account.PublicKey} {requestResult.Result} ");
            
            if (requestResult.Result.Value < 50000000)
            {
                await Airdrop();
            }
        }
        
        public async Task Airdrop()
        {
            var airdropResult = await Web3.Rpc.RequestAirdropAsync(Web3.Account.PublicKey, 100000000);
            var txResult = await Web3.Rpc.ConfirmTransaction(airdropResult.Result, Commitment.Confirmed);
            var balanceResult = await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey);
            Debug.Log($"{Web3.Account.PublicKey} \nairdropResult.Result: {airdropResult.Result}, \ntxResult{txResult} \n balanceResult:{balanceResult} ");
        }
        
        protected async Task<string> GetComponentDataAddress()
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");
            var walletBase = Web3.Wallet;

            if (_dataAddress == null)
            {
                var playerEntityState = await Web3.Rpc.GetAccountInfoAsync(EntityPda);
                if (playerEntityState.Result.Value == null)
                {
                    var tx = new Transaction
                    {
                        FeePayer = Web3.Account,
                        Instructions = new List<TransactionInstruction>
                        {
                            WorldProgram.AddEntity(new AddEntityAccounts()
                            {
                                Payer = Web3.Account.PublicKey,
                                World = new(WorldPda),
                                Entity = new(EntityPda),
                                SystemProgram = SystemProgram.ProgramIdKey
                            }, GetExtraSeed())
                        },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };


                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                var dataAddress = Pda.FindComponentPda(new(EntityPda), GetComponentProgramAddress());

                var componentDataState = await Web3.Rpc.GetAccountInfoAsync(EntityPda);
                if (componentDataState.Result.Value == null)
                {
                    var tx = new Transaction
                    {
                        FeePayer = Web3.Account,
                        Instructions = new List<TransactionInstruction>
                        {
                            WorldProgram.InitializeComponent(new InitializeComponentAccounts()
                            {
                                Payer = Web3.Account,
                                Entity = new PublicKey(EntityPda),
                                Data = dataAddress,
                                ComponentProgram = GetComponentProgramAddress(),
                                SystemProgram = SystemProgram.ProgramIdKey
                            })
                        },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };

                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                _dataAddress = dataAddress;
            }

            return _dataAddress;
        }

        protected async Task<bool> ApplySystem(PublicKey system, object args)
        {
            Dimmer.Visible = true;
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    WorldProgram.ApplySystem(
                        system,
                        new[]
                        {
                            new WorldProgram.EntityType(new(EntityPda),
                                new[] { GetComponentProgramAddress() })
                        },
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args)),
                        Web3.Account.PublicKey
                    )
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Processed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log($"ApplySystem: {result.Result} {JsonConvert.SerializeObject(args)}");

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Processed);

            Dimmer.Visible = false;
            return result.WasSuccessful;
        }

        public async Task SyncTime()
        {
            var slot = await Web3.Rpc.GetSlotAsync(Commitment.Processed);
            var nodeTimestamp = await Web3.Rpc.GetBlockTimeAsync(slot.Result);
            _timeOffset = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (long)nodeTimestamp.Result;

            Debug.Log(_timeOffset);
        }

        public long GetNodeTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _timeOffset;
        }
    }
}