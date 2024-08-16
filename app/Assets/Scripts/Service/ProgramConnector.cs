using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Settlement;
using Settlement.Program;
using Solana.Unity.Programs;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using View.UI;
using World.Program;
// ReSharper disable InconsistentNaming (this is for args passing without renaming etc to match rust)

namespace Service
{
    [Unity.VisualScripting.Singleton]
    public class ProgramConnector : InjectableObject<ProgramConnector>
    {
        [Inject] private SettlementModel _settlement;

        //this comes from program deployment
        private const string WorldPda = "5Fj5HJud66muuDyateWdP2HAPkED7CnyApDQBMreVQQH";
        private const string SettlementProgramAddress = "ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ";
        private const int WorldIndex = 2;

        private SettlementClient _client;

        private SettlementClient Settlement =>
            _client ??= new SettlementClient(Web3.Rpc, Web3.WsRpc, new PublicKey(SettlementProgram.ID));

        private string _dataAddress;
        private string _entityPda;

        private string EntityPda =>
            _entityPda ??= Pda.FindEntityPda(WorldIndex, 0, ExtraSeed);

        private static string ExtraSeed => Web3.Account.PublicKey.Key[..20];


        public async Task EnsureBalance()
        {
            var requestResult = (await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey));
            if (requestResult.Result.Value < 500000000)
            {
                var airdropResult = await Web3.Rpc.RequestAirdropAsync(Web3.Account.PublicKey, 1000000000);
                Debug.Log(JsonConvert.SerializeObject(airdropResult));
                await Web3.Rpc.ConfirmTransaction(airdropResult.Result, Commitment.Confirmed);
            }

            requestResult = (await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey));

            Debug.Log(JsonConvert.SerializeObject(requestResult));
        }

        public async Task<bool> ReloadData()
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");
            var walletBase = Web3.Wallet;

            if (_dataAddress == null)
            {
                //add player entity if doesnt exist

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
                            }, ExtraSeed)
                        },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };


                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
                    Debug.Log(JsonConvert.SerializeObject(result));
                }

                var dataAddress = Pda.FindComponentPda(new(EntityPda), new(SettlementProgramAddress));

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
                                Entity = new(EntityPda),
                                Data = dataAddress,
                                ComponentProgram = new(SettlementProgramAddress),
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

            var rawData = await Settlement.GetSettlementAsync(_dataAddress, Commitment.Processed);

            if (rawData.ParsedResult == null)
                return false;

            _settlement.Set(rawData.ParsedResult);

            return true;
        }

        public async Task<bool> PlaceBuilding(byte x, byte y, byte type)
        {
            return await ApplySystem(new PublicKey("Fgc4uSFUPnhUpwUu7z4siYiBtnkxrwroYVQ2csDo3Q7P"),
                new { x, y, config_index = type });
        }

        public async Task<bool> Wait(int time)
        {
            return await ApplySystem(new PublicKey("ECfKKquvf7PWgvCTAQiYkbDGVjaxhqAN4DFZCAjTUpwx"),
                new { time });
        }

        public async Task<bool> AssignLabour(int labour_index, int building_index)
        {
            return await ApplySystem(new PublicKey("BEc67x2mycQPPeWDLB8r2LCV4TSZCHTfp7rjpjwFwUhH"),
                new { labour_index, building_index });
        }
        
        
        public async Task<bool> Repair(int index)
        {
            return await ApplySystem(new PublicKey("FViANCSUgsHJxK3m1J2d41Yq5yJ13BDkA8eiRwQPFHuJ"), new { index });
        }
        public async Task<bool> Upgrade(int index)
        {
            return await ApplySystem(new PublicKey("FrTthTtkfEWa2zZt4YEHGbL9Hz8hpsSW1hsHHnJXPRd4"), new { index });
        }

        private async Task<bool> ApplySystem(PublicKey system, object args)
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
                                new[] { new PublicKey(SettlementProgramAddress) })
                        },
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args)),
                        Web3.Account.PublicKey
                    )
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log(JsonConvert.SerializeObject(result));

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);

            Dimmer.Visible = false;
            return result.WasSuccessful;
        }

    }
}