using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using Cysharp.Threading.Tasks;
using Hero.Program;
using Newtonsoft.Json;
using Settlement.Program;
using Solana.Unity.Programs;
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
using DelegateAccounts = Hero.Program.DelegateAccounts;
using UndelegateAccounts = Settlement.Program.UndelegateAccounts;

namespace Connectors
{
    [Singleton]
    public abstract class BaseComponentConnector<T> : InjectableObject
    {
        
        private WalletBase Wallet => _delegated
            ? Web3Utils.EphemeralWallet
            : Web3.Wallet;

        protected IRpcClient RpcClient => _delegated
            ? Web3Utils.EphemeralWallet.ActiveRpcClient
            : Web3.Wallet.ActiveRpcClient;

        private static readonly PublicKey DelegationProgram = new("DELeGGvXpWV2fqJUhqcF5ZSYMS4JTLjteaAMARRSaeSh");

        //this comes from program deployment
        private const string
            WorldPda = "H4it5GRk6S2f7sZ9eDm178QhAoFiTby4AzMFBvG5quYL";
        //WorldPda = "5Fj5HJud66muuDyateWdP2HAPkED7CnyApDQBMreVQQH";


        protected const int WorldIndex = 1777;
        //private const int WorldIndex = 2;

        public string EntityPda => _entityPda;
        public string DataAddress => _dataAddress;

        private string _entityPda;
        private long _timeOffset;
        private string _dataAddress;
        private string _seed;
        protected SubscriptionState _sub;
        private bool _delegated;
        private Action<T> _callback;

        public abstract PublicKey GetComponentProgramAddress();

        public async UniTask SetSeed(string value, bool forceCreateEntity = true)
        {
            _seed = value;
            _entityPda = Pda.FindEntityPda(WorldIndex, 0, value);
            await AcquireComponentDataAddress(forceCreateEntity);
        }

        public async UniTask SetEntityPda(string value, bool forceCreateEntity = true)
        {
            _entityPda = value;
            await AcquireComponentDataAddress(forceCreateEntity);
        }


        public void SetDataAddress(string value)
        {
            _dataAddress = value;
        }


        public async UniTask<bool> Delegate()
        {
            var streamingClient = await GetStreamingClient();
            if (_delegated)
                return false;

            var resubscribe = false;
            if (_sub != null)
            {
                await (await GetStreamingClient()).UnsubscribeAsync(_sub);
                resubscribe = true;
            }

            // load account from mainnet to know the real owner
            var dataAcc = await Web3.Wallet.ActiveRpcClient.GetAccountInfoAsync(_dataAddress, Commitment.Processed);

            if (dataAcc.Result.Value?.Owner?.Equals(DelegationProgram) ?? false)
            {
                _delegated = true;

                if (resubscribe)
                    _sub = await (await GetStreamingClient()).SubscribeAccountInfoAsync(_dataAddress, InternalCallback,
                        Commitment.Processed);

                return false;
            }

            var txDelegate = await DelegateTransaction(new(_entityPda), new(_dataAddress));
            var resDelegation = await Wallet.SignAndSendTransaction(txDelegate, true);
            if (resDelegation.WasSuccessful)
            {
                Debug.Log($"Delegate Signature: {resDelegation.Result}");
                await RpcClient.ConfirmTransaction(resDelegation.Result, Commitment.Confirmed);
                _delegated = true;

                if (resubscribe)
                    _sub = await streamingClient.SubscribeAccountInfoAsync(_dataAddress, InternalCallback,
                        Commitment.Processed);
                return true;
            }

            return false;
        }


        public async UniTask<bool> Undelegate()
        {
            var dataAcc = await Web3.Wallet.ActiveRpcClient.GetAccountInfoAsync(_dataAddress, Commitment.Processed);

            if (dataAcc.Result.Value?.Owner == null || dataAcc.Result.Value?.Owner != DelegationProgram)
                return false;

            var streamingClient = await GetStreamingClient();
            var resubscribe = false;
            if (_sub != null)
            {
                await (await GetStreamingClient()).UnsubscribeAsync(_sub);
                resubscribe = true;
            }

            // load ac form mainnet to know the real owner
            if (!dataAcc.Result.Value?.Owner?.Equals(DelegationProgram) ?? false)
            {
                _delegated = false;

                if (resubscribe)
                    _sub = await (await GetStreamingClient()).SubscribeAccountInfoAsync(_dataAddress, InternalCallback,
                        Commitment.Processed);
                return false;
            }


            var txUndelegate = await UndelegateTransaction(new PublicKey(_dataAddress));
            try
            {
                var resUndelegation = await Wallet.SignAndSendTransaction(txUndelegate, true);
                await RpcClient.ConfirmTransaction(resUndelegation.Result, Commitment.Confirmed);
                Debug.Log($"Undelegate Signature: {resUndelegation.Result}");
                if (resUndelegation.WasSuccessful)
                {
                    await RpcClient.ConfirmTransaction(resUndelegation.Result, Commitment.Confirmed);
                    _delegated = false;

                    if (resubscribe)
                        _sub = await streamingClient.SubscribeAccountInfoAsync(_dataAddress, InternalCallback,
                            Commitment.Processed);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return false;
        }

        private async UniTask AcquireComponentDataAddress(bool forceCreateEntity)
        {
            if (Web3.Account == null) throw new NullReferenceException("No Web3 Account");
            var walletBase = Web3.Wallet;

            if (_dataAddress == null)
            {
                var entityState = await RpcClient.GetAccountInfoAsync(_entityPda);
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
                    await RpcClient.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                var dataAddress = Pda.FindComponentPda(new(_entityPda), GetComponentProgramAddress());

                var componentDataState = await RpcClient.GetAccountInfoAsync(dataAddress);
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
                                SystemProgram = SystemProgram.ProgramIdKey,
                                Authority = Web3.Wallet.Account.PublicKey,
                                InstructionSysvarAccount = SysVars.InstructionAccount
                            })
                        },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };

                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await RpcClient.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                _dataAddress = dataAddress;
            }
        }


        public async UniTask<T> LoadData()
        {
            if (string.IsNullOrEmpty(_dataAddress))
                return default;

            var res = await RpcClient.GetAccountInfoAsync(new PublicKey(_dataAddress),
                Commitment.Processed);
            if (!res.WasSuccessful || res.Result.Value == null)
                return default;

            var resultingAccount = DeserialiseBytes(Convert.FromBase64String(res.Result.Value.Data[0]));

            var loadedFromMainnet = !_delegated;
            _delegated = _delegated || res.Result.Value.Owner == DelegationProgram;
            if (loadedFromMainnet && _delegated)
            {
                var rollupData = await LoadData();

                if (rollupData == null)
                {
                    await CloneToRollup();
                    rollupData = await LoadData();
                }

                return rollupData; //reload data from rollup
            }

            Debug.Log($"Data:\n {JsonConvert.SerializeObject(resultingAccount)}");
            return resultingAccount;
        }

        public virtual UniTask CloneToRollup()
        {
            //throw new NotImplementedException();
            return UniTask.FromResult(true);
        }

        public async UniTask Subscribe(Action<T> callback)
        {
            Debug.Log("Subscribing to data address: " + _dataAddress);
            var streamingClient = await GetStreamingClient();
            if (string.IsNullOrEmpty(_dataAddress))
                return;
            _callback = callback;
            if (streamingClient.State != WebSocketState.Open)
            {
                Debug.LogError(
                    $"Unable to subscribe to data address: {streamingClient.NodeAddress} On: ({streamingClient.NodeAddress})");
                return;
            }

            _sub = await streamingClient.SubscribeAccountInfoAsync(_dataAddress, InternalCallback,
                Commitment.Processed);
            Debug.Log($"Subscribed to data address: {_dataAddress}, on {streamingClient.NodeAddress}");
        }

        private async void InternalCallback(SubscriptionState s, ResponseValue<AccountInfo> e)
        {
            try
            {
                Debug.Log("Data account updated: " + _dataAddress);
                // TODO: This is a hack to make sure we are on the main thread when the callback is called.
                // Can be removed after updating to the master version of the Unity SDK.
                await UniTask.SwitchToMainThread();
                var parsingResult = default(T);
                if (e.Value?.Data?.Count > 0)
                    parsingResult = DeserialiseBytes(Convert.FromBase64String(e.Value.Data[0]));
                _callback?.Invoke(parsingResult);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public async UniTask Unsubscribe()
        {
            _callback = null;
            if (_sub != null)
                await _sub.UnsubscribeAsync();
        }

        protected abstract T DeserialiseBytes(byte[] value);

        protected async virtual UniTask<bool> ApplySystem(PublicKey systemAddress, object args,
            Dictionary<PublicKey, PublicKey> extraEntities = null, bool useDataAddress = false,
            AccountMeta[] accounts = null, bool ignoreSession = false)
        {
            
            var systemApplicationInstruction =
                useDataAddress
                    ? GetSystemApplicationInstructionFromDataAddress(
                        systemAddress,
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args)),
                        ignoreSession
                    )
                    : CreateSystemApplicationInstructionFromEntities(systemAddress, args, extraEntities, ignoreSession);


            if (accounts != null)
                foreach (var account in accounts)
                    systemApplicationInstruction.Keys.Add(account);

            Debug.Log($"Applying System {systemAddress} with args.. :  {JsonConvert.SerializeObject(args)}");
            return await ExecuteSystemApplicationInstruction(systemApplicationInstruction, ignoreSession);
        }

        private TransactionInstruction CreateSystemApplicationInstructionFromEntities(PublicKey systemAddress,
            object args, Dictionary<PublicKey, PublicKey> extraEntities, bool ignoreSession = false)
        {
            var componentProgramAddress = GetComponentProgramAddress();

            var input = new WorldProgram.EntityType[]
            {
                new(new PublicKey(_entityPda), new[] { componentProgramAddress })
            };

            if (extraEntities != null)
                input = input
                    .Concat(extraEntities.Select(pair => new WorldProgram.EntityType(pair.Key, new[] { pair.Value }))
                        .ToArray()).ToArray();


            var authority = Web3Utils.SessionToken == null || ignoreSession
                ? Web3.Wallet.Account.PublicKey
                : Web3Utils.SessionWallet.Account.PublicKey;
            
            return WorldProgram.ApplySystem(
                new PublicKey(WorldPda),
                systemAddress,
                input,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args)),
                authority,
                ignoreSession?null:Web3Utils.SessionWallet?.SessionTokenPDA);
        }

        private TransactionInstruction GetSystemApplicationInstructionFromDataAddress(
            PublicKey systemAddress,
            byte[] args, bool ignoreSession = false)
        {
            TransactionInstruction transactionInstruction;
            if (Web3Utils.SessionToken?.SessionSigner != null && !ignoreSession)
                transactionInstruction = WorldProgram.ApplyWithSession(new ApplyWithSessionAccounts()
                {
                    BoltSystem = systemAddress,
                    Authority = Web3Utils.SessionWallet.Account.PublicKey,
                    World = new PublicKey(WorldPda),
                    SessionToken = Web3Utils.SessionWallet?.SessionTokenPDA
                }, args, new PublicKey(WorldProgram.ID));
            else
                transactionInstruction = WorldProgram.Apply(new ApplyAccounts()
                {
                    BoltSystem = systemAddress,
                    Authority = Web3.Account.PublicKey,
                    World = new PublicKey(WorldPda)
                }, args, new PublicKey(WorldProgram.ID));

            transactionInstruction.Keys.Add(AccountMeta.ReadOnly(GetComponentProgramAddress(), false));
            transactionInstruction.Keys.Add(AccountMeta.Writable(new PublicKey(_dataAddress), false));
            transactionInstruction.Keys.Add(AccountMeta.ReadOnly(new PublicKey(WorldProgram.ID), false));

            return transactionInstruction;
        }


        private async UniTask<bool> ExecuteSystemApplicationInstruction(
            TransactionInstruction systemApplicationInstruction, bool ignoreSession = false)
        {
            var walletAccount = Web3Utils.SessionToken == null || ignoreSession ? Wallet.Account : Web3Utils.SessionWallet.Account;
            var signers = new List<Account>
                {
                    walletAccount
                };

            var blockHashResponse = await RpcClient.GetLatestBlockHashAsync(Commitment.Processed);
            if (!blockHashResponse.WasSuccessful || blockHashResponse.Result?.Value?.Blockhash == null)
                throw new Exception("Failed to get latest blockhash");
            var blockhash = blockHashResponse.Result.Value.Blockhash;
            var transaction = new TransactionBuilder()
                .SetFeePayer(walletAccount)
                .SetRecentBlockHash(blockhash)
                .AddInstruction(systemApplicationInstruction)
                .AddInstruction(ComputeBudgetProgram.SetComputeUnitLimit(1000000)) //be generous for now
                .Build(signers);

            var signature = await RpcClient.SendTransactionAsync(transaction, true, Commitment.Confirmed);
            if (!signature.WasSuccessful)
            {

                string errorMessage = signature.Reason;
                errorMessage += "\n" + signature.RawRpcResponse;
                if (signature.ErrorData != null)
                {
                    errorMessage += "\n" + string.Join("\n", signature.ErrorData.Logs);
                }

                Debug.LogError(errorMessage);
                return false;
            }
            
            await RpcClient.ConfirmTransaction(signature.Result, Commitment.Confirmed);
            Debug.Log($"System Application Result: {signature.WasSuccessful} {signature.Result}");
            return true;
        }


        // var latestBlockHash = await RpcClient.GetLatestBlockHashAsync(commitment: Commitment.Processed);
        //     var tx = new Transaction
        //     {
        //         FeePayer = Web3.Account,
        //         Instructions = new List<TransactionInstruction>
        //         {
        //             systemApplicationInstruction,
        //             ComputeBudgetProgram.SetComputeUnitLimit(500000)
        //         },
        //         RecentBlockHash = latestBlockHash.Result.Value.Blockhash
        //     };
        //
        //     var signedTx = await Web3.Wallet.SignTransaction(tx);
        //     var result = await RpcClient.SendTransactionAsync(
        //         Convert.ToBase64String(signedTx.Serialize()),
        //         skipPreflight: true, preFlightCommitment: Commitment.Confirmed);
        //
        //     Debug.Log($"System Application Result: {result.WasSuccessful} {result.Result}");
        //
        //     if (!result.WasSuccessful)
        //         Debug.LogError($"System Application ErrorReason: {result.Reason}");
        //
        //     await RpcClient.ConfirmTransaction(result.Result, Commitment.Processed);
        //     return result.WasSuccessful;
        // }


        public async UniTask<Transaction> DelegateTransaction(PublicKey entityPda, PublicKey playerDataPda)
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
            DelegateAccounts delegateAccounts = new()
            {
                Payer = Web3.Account,
                Entity = entityPda,
                Account = playerDataPda,
                DelegationProgram = DelegationProgram,
                DelegationRecord = FindDelegationProgramPda("delegation", playerDataPda),
                DelegationMetadata = FindDelegationProgramPda("delegation-metadata", playerDataPda),
                Buffer = FindBufferPda("buffer", playerDataPda, GetComponentProgramAddress()),
                OwnerProgram = GetComponentProgramAddress(),
                SystemProgram = SystemProgram.ProgramIdKey
            };
            var ixDelegate = HeroProgram.Delegate(delegateAccounts, 3000, null, GetComponentProgramAddress());
            tx.Add(ixDelegate);

            return tx;
        }

        public async UniTask<Transaction> UndelegateTransaction(PublicKey playerDataPda)
        {
            var tx = new Transaction()
            {
                FeePayer = Web3Utils.EphemeralWallet.Account,
                Instructions = new List<TransactionInstruction>(),
                RecentBlockHash =
                    await Web3Utils.EphemeralWallet.GetBlockHash(commitment: Commitment.Confirmed, useCache: false)
            };
            // Increase compute unit limit
            tx.Instructions.Add(ComputeBudgetProgram.SetComputeUnitLimit(75000));
            tx.Instructions.Add(ComputeBudgetProgram.SetComputeUnitPrice(100000));

            // Delegate the player data pda
            UndelegateAccounts undelegateAccounts = new()
            {
                Payer = Web3.Account,
                DelegatedAccount = playerDataPda
            };
            tx.Add(SettlementProgram.Undelegate(undelegateAccounts));

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

        protected async UniTask<IStreamingRpcClient> GetStreamingClient()
        {
            var wallet = _delegated ? Web3Utils.EphemeralWallet : Web3.Wallet;
            if (wallet.ActiveStreamingRpcClient.State != WebSocketState.Open)
                await wallet.AwaitWsRpcConnection();

            return wallet.ActiveStreamingRpcClient;
        }
    }
}