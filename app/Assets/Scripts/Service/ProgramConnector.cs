using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Settlement;
using Settlement.Program;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using World.Program;

namespace Service
{
    [Unity.VisualScripting.Singleton]
    public class ProgramConnector : InjectableObject<ProgramConnector>
    {
        [Inject] private SettlementModel _settlement;

        private SettlementClient _client;

        private SettlementClient Settlement =>
            _client ?? (_client = new(Web3.Rpc, Web3.WsRpc, new PublicKey(SettlementProgram.ID)));


        private string dataOfTheComponentAddress = "DwLoUS41vpHQ6oCLpinaXWzbNja7NmmQfTk1z6FiycEz";
        private int _worldIndex = 2;

        public async Task Initialise()
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");

            var walletBase = Web3.Wallet;

            var worldPDA = "5Fj5HJud66muuDyateWdP2HAPkED7CnyApDQBMreVQQH";
            //
            var extraSeed = Web3.Account.PublicKey.Key.Substring(0, 20);
            var entityPda = Pda.FindEntityPda(_worldIndex, 0, extraSeed);
            // var createEntity = new AddEntityAccounts()
            // {
            //     Payer = Web3.Account.PublicKey,
            //     World = new(worldPDA),
            //     Entity = entityPda,
            //     SystemProgram = SystemProgram.ProgramIdKey
            // };
            //
            // Debug.Log(JsonConvert.SerializeObject(createEntity));
            // Debug.Log(extraSeed);
            //
            // var tx = new Transaction
            // {
            //     FeePayer = Web3.Account,
            //     Instructions = new List<TransactionInstruction>
            //     {
            //         WorldProgram.AddEntity(createEntity, extraSeed)
            //     },
            //     RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            // };
            //
            // var result = await walletBase.SignAndSendTransaction(tx, true);
            // Debug.Log(JsonConvert.SerializeObject(result));
            //
            //
            // var componentProgramAddress = new PublicKey("ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ");
            // var dataOfTheComponentAddress = Pda.FindComponentPda(entityPda, componentProgramAddress);
            // var initComponent = new InitializeComponentAccounts()
            // {
            //     Payer = Web3.Account,
            //     Entity = entityPda,
            //     Data = dataOfTheComponentAddress,
            //     ComponentProgram = componentProgramAddress,
            //     SystemProgram = SystemProgram.ProgramIdKey
            // };
            //
            //
            // tx = new Transaction
            // {
            //     FeePayer = Web3.Account,
            //     Instructions = new List<TransactionInstruction>
            //     {
            //         WorldProgram.InitializeComponent(initComponent)
            //     },
            //     RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            // };
            //
            // result = await walletBase.SignAndSendTransaction(tx, true);
            // Debug.Log(JsonConvert.SerializeObject(result));
            //
            // Debug.Log("dataOfTheComponentAddress: " + dataOfTheComponentAddress);
            //
            // await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);

            var rawData = await Settlement.GetSettlementAsync(dataOfTheComponentAddress, Commitment.Processed);

            Debug.Log(JsonConvert.SerializeObject(rawData.ParsedResult));
        }

        public async Task<bool> ReloadData()
        {
            var rawData = await Settlement.GetSettlementAsync(dataOfTheComponentAddress);

            if (rawData.ParsedResult == null)
                return false;

            _settlement.Set(rawData.ParsedResult);

            return true;
        }

        public async Task<bool> PlaceBuilding(byte x, byte y, byte type)
        {
            var extraSeed = Web3.Account.PublicKey.Key.Substring(0, 20);
            var entityPda = Pda.FindEntityPda(_worldIndex, 0, extraSeed);
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    WorldProgram.ApplySystem(
                        new("Fgc4uSFUPnhUpwUu7z4siYiBtnkxrwroYVQ2csDo3Q7P"),
                        new[]
                        {
                            new WorldProgram.EntityType(entityPda, new[]
                            {
                                new PublicKey("ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ")
                            })
                        },
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { x, y, config_index = type })),
                        Web3.Account.PublicKey
                    )
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log(JsonConvert.SerializeObject(result));

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
            return result.WasSuccessful;
        }

        public async Task<bool> Wait(int time)
        {
            var extraSeed = Web3.Account.PublicKey.Key.Substring(0, 20);
            var entityPda = Pda.FindEntityPda(_worldIndex, 0, extraSeed);
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    WorldProgram.ApplySystem(
                        new PublicKey("ECfKKquvf7PWgvCTAQiYkbDGVjaxhqAN4DFZCAjTUpwx"),
                        new[]
                        {
                            new WorldProgram.EntityType(entityPda, new[]
                            {
                                new PublicKey("ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ")
                            })
                        },
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { time })),
                        Web3.Account.PublicKey
                    )
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log(JsonConvert.SerializeObject(result));

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);

            return result.WasSuccessful;
        }

        public async Task<bool> AssignLabour(int labour_index, int building_index)
        {
            var extraSeed = Web3.Account.PublicKey.Key.Substring(0, 20);
            var entityPda = Pda.FindEntityPda(_worldIndex, 0, extraSeed);
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    WorldProgram.ApplySystem(
                        new PublicKey("BEc67x2mycQPPeWDLB8r2LCV4TSZCHTfp7rjpjwFwUhH"),
                        new[]
                        {
                            new WorldProgram.EntityType(entityPda, new[]
                            {
                                new PublicKey("ARDmmVcLaNW6b9byetukTFFriUAjpw7CkSfnapR86QfZ")
                            })
                        },
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { labour_index, building_index })),
                        Web3.Account.PublicKey
                    )
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log(JsonConvert.SerializeObject(result));

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);

            return result.WasSuccessful;
        }


        public async Task<bool> ResetAccounts()
        {
            throw new NotImplementedException();
        }
    }
}