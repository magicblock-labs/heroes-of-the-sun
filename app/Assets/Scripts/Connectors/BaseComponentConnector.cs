using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Hero.Program;
using Newtonsoft.Json;
using Solana.Unity.Programs;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Messages;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils;
using Utils.Injection;
using World.Program;

namespace Connectors
{
    [Singleton]
    public abstract class BaseComponentConnector<T> : InjectableObject
    {
        private static IRpcClient RpcClient => _delegated
                ? Web3Utils.EphemeralWallet.ActiveRpcClient
                : Web3.Wallet.ActiveRpcClient;

        public static readonly PublicKey DelegationProgram = new("DELeGGvXpWV2fqJUhqcF5ZSYMS4JTLjteaAMARRSaeSh");


        //this comes from program deployment
        private const string
            //WorldPda = "7U6fFqwbzCULK7y1PXUe5nqQpgKiFwoFiMR4vTSrYdkt";
            WorldPda = "5Fj5HJud66muuDyateWdP2HAPkED7CnyApDQBMreVQQH";


        //private const int WorldIndex = 1709;
        private const int WorldIndex = 2;

        public string EntityPda => _entityPda;
        public string DataAddress => _dataAddress;

        private string _entityPda;
        private long _timeOffset;
        private string _dataAddress;
        private string _seed;
        private static bool _delegated;

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

        public async Task Delegate()
        {
            if (_delegated)
                return;

            // Delegate the data PDA if needed
            var dataAcc = await Web3.Rpc.GetAccountInfoAsync(_dataAddress, Commitment.Processed);
            if (dataAcc.Result.Value != null
                && !dataAcc.Result.Value.Owner.Equals(DelegationProgram)
                && !Web3.Rpc.NodeAddress.ToString()
                    .Equals(Web3Utils.EphemeralWallet.ActiveRpcClient.NodeAddress.ToString()))
            {
                var txDelegate = await DelegateTransaction(new(_entityPda), new(_dataAddress));
                var resDelegation = await Web3.Wallet.SignAndSendTransaction(txDelegate);
                if (resDelegation.WasSuccessful)
                {
                    Debug.Log($"Delegate Signature: {resDelegation.Result}");
                    await Web3.Rpc.ConfirmTransaction(resDelegation.Result, Commitment.Confirmed);
                }
            }

            _delegated = true;
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
            if (!res.WasSuccessful || res.Result.Value == null)
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
            Dictionary<PublicKey, PublicKey> extraEntities = null, bool useDataAddress = false,
            AccountMeta[] accounts = null)
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

            systemApplicationInstruction.Keys.Add(AccountMeta.ReadOnly(new(WorldPda), false));

            if (accounts != null)
                foreach (var account in accounts)
                    systemApplicationInstruction.Keys.Add(account);

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

            var signedTx = await Web3.Wallet.SignTransaction(tx);
            var result = await RpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTx.Serialize()),
                skipPreflight: true, preFlightCommitment: Commitment.Processed);

            Debug.Log($"System Application Result: {result.WasSuccessful} {result.Result}");

            await Web3.Rpc.ConfirmTransaction(result.Result, Commitment.Processed);
            return result.WasSuccessful;
        }


        public async Task<Transaction> DelegateTransaction(PublicKey entityPda, PublicKey playerDataPda)
        {
            var tx = new Transaction()
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>(),
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };
            // Increase compute unit limit
            tx.Instructions.Add(ComputeBudgetProgram.SetComputeUnitLimit(75000));
            tx.Instructions.Add(ComputeBudgetProgram.SetComputeUnitPrice(100000));

            // Delegate the player data pda
            DelegateAccounts heroProgram = new()
            {
                Payer = Web3.Account,
                Entity = entityPda,
                Account = playerDataPda,
                DelegationProgram = DelegationProgram,
                DelegationRecord = FindDelegationProgramPda("delegation", playerDataPda),
                // DelegationMetadata = FindDelegationProgramPda("delegation-metadata", playerDataPda),
                Buffer = FindBufferPda("buffer", playerDataPda, GetComponentProgramAddress()),
                OwnerProgram = GetComponentProgramAddress(),
                SystemProgram = SystemProgram.ProgramIdKey
            };
            var ixDelegate = HeroProgram.Delegate(heroProgram, 0, 3000, null);
            tx.Add(ixDelegate);

            return tx;
        }

        public static PublicKey FindDelegationProgramPda(string seed, PublicKey account)
        {
            PublicKey.TryFindProgramAddress(new[]
            {
                Encoding.UTF8.GetBytes(seed), account.KeyBytes
            }, DelegationProgram, out var pda, out _);
            return pda;
        }

        public static PublicKey FindBufferPda(string seed, PublicKey account, PublicKey owner)
        {
            PublicKey.TryFindProgramAddress(new[]
            {
                Encoding.UTF8.GetBytes(seed), account.KeyBytes
            }, owner, out var pda, out _);
            return pda;
        }
    }
}