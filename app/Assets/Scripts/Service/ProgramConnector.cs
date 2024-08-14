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

        public async Task ResetAccounts()
        {
            throw new NotImplementedException();
        }

        private string dataOfTheComponentAddress = "DwLoUS41vpHQ6oCLpinaXWzbNja7NmmQfTk1z6FiycEz";

        public async Task Initialise()
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");

            var walletBase = Web3.Wallet;

            var worldPDA = "5Fj5HJud66muuDyateWdP2HAPkED7CnyApDQBMreVQQH";
            //
            var extraSeed = Web3.Account.PublicKey.Key.Substring(0, 20);
            var entityPda = Pda.FindEntityPda(2, 0, extraSeed);
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


            // var initializeNewWorldAccounts = new InitializeNewWorldAccounts
            // {
            //     Payer = Web3.Account.PublicKey,
            //     World = GetWorldPda(Web3.Account.PublicKey),
            //     Registry = new PublicKey("EHLkWwAT9oebVv9ht3mtqrvHhRVMKrt54tF3MfHTey2K"),
            //     SystemProgram = SystemProgram.ProgramIdKey
            // };
            //
            // Debug.Log("initializeNewWorldAccounts :" + JsonConvert.SerializeObject(initializeNewWorldAccounts));
            //
            // var tx = new Transaction
            // {
            //     FeePayer = Web3.Account,
            //     Instructions = new List<TransactionInstruction>
            //     {
            //         WorldProgram.InitializeNewWorld(initializeNewWorldAccounts)
            //     },
            //     RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            // };
            //
            // PublicKey GetWorldPda(PublicKey accountId)
            // {
            //     PublicKey.TryFindProgramAddress(new[]
            //         {
            //             Encoding.UTF8.GetBytes("world"),
            //             accountId.KeyBytes
            //         },
            //         new(WorldProgram.ID), out var pda, out _);
            //
            //     Debug.Log(pda);
            //     return pda;
            // }
            //
            // var result = await walletBase.SignAndSendTransaction(tx);
            //
            // if (result.WasSuccessful)
            // {
            //     Debug.Log("!!!: " + GetWorldPda(Web3.Account.PublicKey));
            // }
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
            var entityPda = Pda.FindEntityPda(2, 0, extraSeed);
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
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { x, y, config_index=type })),
                        Web3.Account.PublicKey
                    )
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log(JsonConvert.SerializeObject(result));
            return result.WasSuccessful;
        }
    }
}