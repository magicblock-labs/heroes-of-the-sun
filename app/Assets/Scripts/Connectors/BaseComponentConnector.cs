using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Solana.Unity.Programs;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using World.Program;

namespace Connectors
{
    [Singleton]
    public abstract class BaseComponentConnector<T> : InjectableObject
    {
        //this comes from program deployment
        private const string
            WorldPda = "5Fj5HJud66muuDyateWdP2HAPkED7CnyApDQBMreVQQH"; //"GvMv6N5UF8ctteapSXMJUh2GXmXb4a7hRHWNmi69PTA8";

        private const int WorldIndex = 2; //1318;

        public string EntityPda => _entityPda;
        public string DataAddress => _dataAddress;

        private string _entityPda;
        private long _timeOffset;
        private string _dataAddress;
        private string _seed;

        public abstract PublicKey GetComponentProgramAddress();

        public async Task SetSeed(string value, bool forceCreateEntity = true)
        {
            _seed = value;
            _entityPda = Pda.FindEntityPda(WorldIndex, 0, value);
            await AcquireComponentDataAddress(forceCreateEntity);
        }

        public async Task SetEntityPda(string value, bool forceCreateEntity = true)
        {
            _entityPda = value;
            await AcquireComponentDataAddress(forceCreateEntity);
        }
        

        public void SetDataAddress(string value)
        {
            _dataAddress = value;
        }

        public async Task AcquireComponentDataAddress(bool forceCreateEntity)
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");
            var walletBase = Web3.Wallet;

            if (_dataAddress == null)
            {
                var entityState = await Web3.Rpc.GetAccountInfoAsync(_entityPda);
                if (entityState.Result.Value == null)
                {
                    if (!forceCreateEntity)
                        return;

                    var tx = new Transaction
                    {
                        FeePayer = Web3.Account,
                        Instructions = new List<TransactionInstruction>
                        {
                            WorldProgram.AddEntity(new AddEntityAccounts()
                            {
                                Payer = Web3.Account.PublicKey,
                                World = new(WorldPda),
                                Entity = new(_entityPda),
                                SystemProgram = SystemProgram.ProgramIdKey
                            }, _seed)
                        },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };

                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                var dataAddress = Pda.FindComponentPda(new(_entityPda), GetComponentProgramAddress());

                var componentDataState = await Web3.Rpc.GetAccountInfoAsync(_entityPda);
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
                                Entity = new PublicKey(_entityPda),
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
        }


        public async Task<T> LoadData()
        {
            if (string.IsNullOrEmpty(_dataAddress))
                return default;

            var res = await Web3.Rpc.GetAccountInfoAsync(new PublicKey(_dataAddress),
                Commitment.Processed);
            if (!res.WasSuccessful)
                return default;

            var resultingAccount = DeserialiseBytes(Convert.FromBase64String(res.Result.Value.Data[0]));
            Debug.Log($"Data:\n {JsonConvert.SerializeObject(resultingAccount)}");
            return resultingAccount;
        }

        public async Task Subscribe(Action<SubscriptionState, ResponseValue<AccountInfo>, T> callback)
        {
            Debug.Log("Subscribing to data address: " + _dataAddress);
            if (string.IsNullOrEmpty(_dataAddress))
                return;

            await Web3.Wallet.ActiveStreamingRpcClient.SubscribeAccountInfoAsync(_dataAddress, async (s, e) =>
            {
                Debug.Log("Data account updated: " + _dataAddress);
                // TODO: This is a hack to make sure we are on the main thread when the callback is called.
                // Can be removed after updating to the master version of the Unity SDK.
                await UniTask.SwitchToMainThread();
                var parsingResult = default(T);
                if (e.Value?.Data?.Count > 0)
                    parsingResult = DeserialiseBytes(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, Commitment.Processed);
        }

        protected abstract T DeserialiseBytes(byte[] value);

        protected async Task<bool> ApplySystem(PublicKey systemAddress, object args,
            Dictionary<PublicKey, PublicKey> extraEntities = null, bool useDataAddress = false)
        {
            var componentProgramAddress = GetComponentProgramAddress();
            if (componentProgramAddress == null)
                throw new Exception("component program address not set");

            var systemApplicationInstruction =
                useDataAddress
                    ? GetSystemApplicationInstructionFromDataAddress(
                        new ApplyAccounts()
                        {
                            BoltSystem = systemAddress,
                            ComponentProgram = componentProgramAddress,
                            BoltComponent = new PublicKey(_dataAddress),
                            Authority = Web3.Account.PublicKey,
                            InstructionSysvarAccount = SysVars.InstructionAccount
                        },
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args))
                    )
                    : GetSystemApplicationInstructionFromEntities(componentProgramAddress, systemAddress, args,
                        extraEntities);


            Debug.Log($"Applying System with args.. :  {JsonConvert.SerializeObject(args)}");
            return await ExecuteSystemApplicationInstruction(systemApplicationInstruction);
        }

        private TransactionInstruction GetSystemApplicationInstructionFromEntities(PublicKey componentProgramAddress,
            PublicKey system, object args, Dictionary<PublicKey, PublicKey> extraEntities = null)
        {
            var entities = new[]
            {
                new WorldProgram.EntityType(new(_entityPda),
                    new[] { componentProgramAddress })
            };

            if (extraEntities != null)
                entities = entities.Concat(
                    extraEntities.Select(kvp => new WorldProgram.EntityType(kvp.Key,
                        new[] { kvp.Value })).ToArray()
                ).ToArray();

            return WorldProgram.ApplySystem(
                system,
                entities,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args)),
                Web3.Account.PublicKey
            );
        }

        private TransactionInstruction GetSystemApplicationInstructionFromDataAddress(
            ApplyAccounts accounts,
            byte[] args)
        {
            List<AccountMeta> accountMetaList = new List<AccountMeta>()
            {
                AccountMeta.ReadOnly(accounts.ComponentProgram, false),
                AccountMeta.ReadOnly(accounts.BoltSystem, false),
                AccountMeta.Writable(accounts.BoltComponent, false),
                AccountMeta.ReadOnly(accounts.Authority, false),
                AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)
            };
            byte[] numArray = new byte[1200];
            int offset1 = 0;
            numArray.WriteU64(16258613031726085112UL, offset1);
            int offset2 = offset1 + 8;
            numArray.WriteS32(args.Length, offset2);
            int offset3 = offset2 + 4;
            numArray.WriteSpan((ReadOnlySpan<byte>)args, offset3);
            int length = offset3 + args.Length;
            byte[] destinationArray = new byte[length];
            Array.Copy((Array)numArray, (Array)destinationArray, length);
            return new TransactionInstruction()
            {
                Keys = (IList<AccountMeta>)accountMetaList,
                ProgramId = new PublicKey("WorLD15A7CrDwLcLy4fRqtaTb9fbd8o8iqiEMUDse2n").KeyBytes,
                Data = destinationArray
            };
        }


        private static async Task<bool> ExecuteSystemApplicationInstruction(
            TransactionInstruction systemApplicationInstruction)
        {
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    systemApplicationInstruction
                },
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Processed, useCache: false)
            };

            var result = await Web3.Wallet.SignAndSendTransaction(tx, true);
            Debug.Log($"System Application Result: {result.WasSuccessful} {result.Result}");

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Processed);
            return result.WasSuccessful;
        }
    }
}