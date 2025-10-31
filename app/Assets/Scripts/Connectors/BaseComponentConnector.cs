using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
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
using Component = Bolt.Component;
using DelegateAccounts = Hero.Program.DelegateAccounts;
using UndelegateAccounts = Settlement.Program.UndelegateAccounts;

/*

  Make sure directly use all the functions from Bolt (ApplySystem, InitializeComponent, DelegateComponent, DestroyComponent) and not custom client-side code. Replace the ProgramIDs with new System(bundleProgramID, "system_name_in_snake_case") or new Component(bundleProgramID, "component_name")

 */

namespace Connectors
{
    public abstract class BaseComponentConnector<T> : InjectableObject
    {
        private WalletBase Wallet => _delegated
            ? Web3Utils.EphemeralWallet
            : Web3.Wallet;

        protected IRpcClient RpcClient => _delegated
            ? Web3Utils.EphemeralWallet?.ActiveRpcClient
            : Web3.Wallet?.ActiveRpcClient;

        private static readonly PublicKey DelegationProgram = new("DELeGGvXpWV2fqJUhqcF5ZSYMS4JTLjteaAMARRSaeSh");
        private static readonly PublicKey BundleId = new("Cjca6tWWGx77ki6rRinErcoZJEvJHNxfPaut8DoKJHQ5");

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
        private const string DataAddressCache = nameof(DataAddressCache);

        public abstract PublicKey GetComponentProgramAddress();
        public abstract string GetComponentName();

        public async UniTask SetSeed(string value, bool forceCreateEntity = true)
        {
            _seed = value;
            _entityPda = Pda.FindEntityPda(WorldIndex, 0, value);
            await AcquireComponentDataAddress(forceCreateEntity);
        }

        public async UniTask SetEntityPda(string value, bool forceCreateEntity = true, bool publicComponent = false)
        {
            _entityPda = value;
            Debug.Log("SetEntityPda: " + _entityPda);
            await AcquireComponentDataAddress(forceCreateEntity, publicComponent);
        }


        public void SetDataAddress(string value)
        {
            _dataAddress = value;
            Debug.Log("SetDataAddress: " + _dataAddress);
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

            var baseWallet = Web3Utils.SessionWallet?.Account?.PublicKey == null
                ? Web3.Wallet
                : Web3Utils.SessionWallet;

            var txDelegate = await DelegateTransaction(new(_entityPda), new(_dataAddress));
            var resDelegation = await baseWallet.SignAndSendTransaction(txDelegate, true);
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
                var baseWallet = Web3Utils.SessionWallet?.Account?.PublicKey == null
                    ? Web3.Wallet
                    : Web3Utils.SessionWallet;

                var resUndelegation = await baseWallet.SignAndSendTransaction(txUndelegate, true);
                await RpcClient.ConfirmTransaction(resUndelegation.Result, Commitment.Confirmed);

                Debug.Log($"Undelegate Signature: {resUndelegation.Result}");

                if (resUndelegation.WasSuccessful)
                {
                    var tx = await RpcClient.GetTransactionAsync(resUndelegation.Result);
                    var messages = tx.Result.Meta.LogMessages;
                    string scheduledTx = null;
                    foreach (var message in messages)
                    {
                        Debug.Log($"Message: {message}");
                        if (message.Contains("signature"))
                        {
                            scheduledTx = message.Split(": ")[1];
                            Debug.Log($"scheduledTx: {scheduledTx}");
                            break;
                        }
                    }

                    await RpcClient.ConfirmTransaction(scheduledTx, Commitment.Confirmed);
                    tx = await RpcClient.GetTransactionAsync(scheduledTx, Commitment.Processed);
                    messages = tx.Result.Meta.LogMessages;
                    foreach (var message in messages)
                    {
                        Debug.Log($"Message: {message}");
                        if (message.Contains("signature"))
                        {
                            scheduledTx = message.Split(": ")[1];
                            Debug.Log($"scheduledTx: {scheduledTx}");
                            break;
                        }
                    }

                    await Web3.Wallet.ActiveRpcClient.ConfirmTransaction(scheduledTx, Commitment.Confirmed);

                    Debug.Log($"Undelegate Signature: {scheduledTx}");

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

        private async UniTask AcquireComponentDataAddress(bool forceCreateEntity, bool publicComponent = true)
        {
            if (_dataAddress != null)
                return;

            var walletBase = Web3Utils.SessionToken == null //|| true
                ? Web3.Wallet
                : Web3Utils.SessionWallet;

            if (walletBase == null) throw new NullReferenceException("No Web3 Account");

            var payer = walletBase.Account;

            // _dataAddress =
            //     PlayerPrefs.GetString($"{DataAddressCache}:{_entityPda}{GetComponentProgramAddress()}", null);
            if (string.IsNullOrEmpty(_dataAddress))
            {
                var entityState = await RpcClient.GetAccountInfoAsync(_entityPda, Commitment.Processed);
                if (entityState.Result.Value == null)
                {
                    if (!forceCreateEntity)
                        return;

                    if (_seed == null) //this basically means entity WAS created externally, but very recenlty
                    {
                        while (entityState.Result.Value == null)
                        {
                            await Task.Delay(2000);
                            entityState = await RpcClient.GetAccountInfoAsync(_entityPda, Commitment.Processed);
                        }
                    }
                    else
                    {
                        var tx = new Transaction
                        {
                            FeePayer = payer,
                            Instructions = new List<TransactionInstruction>
                            {
                                WorldProgram.AddEntity(new AddEntityAccounts()
                                {
                                    Payer = payer.PublicKey,
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
                }

                var dataAddress = Pda.FindComponentPda(new(_entityPda), GetComponentProgramAddress());

                var componentDataState = await RpcClient.GetAccountInfoAsync(dataAddress, Commitment.Processed);
                if (componentDataState.Result.Value == null)
                {
                    var component = new Bolt.Component(BundleId, GetComponentName());
                    var initializeComponent = await Bolt.World.InitializeComponent(payer, new PublicKey(_entityPda),
                        component, "", publicComponent ? new(WorldProgram.ID) : payer.PublicKey);
                    var tx = new Transaction
                    {
                        FeePayer = payer,
                        Instructions = new List<TransactionInstruction>
                            { initializeComponent.Instruction },
                        RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
                    };

                    var result = await walletBase.SignAndSendTransaction(tx, true);
                    await RpcClient.ConfirmTransaction(result.Result, Commitment.Confirmed);
                }

                _dataAddress = dataAddress;
                PlayerPrefs.SetString($"{DataAddressCache}:{_entityPda}{GetComponentProgramAddress()}", dataAddress);
            }
        }

        public virtual async UniTask<T> LoadData()
        {
            if (string.IsNullOrEmpty(_dataAddress))
                return default;

            if (PlayerPrefs.HasKey(DataAddress))
            {
                var cached = PlayerPrefs.GetString(DataAddress);
                var cachedBytes = Convert.FromBase64String(cached);
                return DeserialiseBytes(cachedBytes);
            }

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

        protected virtual async UniTask<bool> ApplySystem(string systemName, object args,
            Dictionary<PublicKey, Bolt.Component> extraEntities = null, AccountMeta[] extraAccounts = null,
            bool forceMainWalletSigner = false)
        {
            var authority = forceMainWalletSigner || Web3Utils.SessionWallet?.Account?.PublicKey == null
                ? Web3.Wallet.Account.PublicKey
                : Web3Utils.SessionWallet.Account.PublicKey;

            var systemInput = new List<(PublicKey entity, Bolt.Component[] components, string[] seeds)?>
            {
                (new PublicKey(_entityPda), new[] { GetComponent() }, Array.Empty<string>())
            };

            if (extraEntities != null)
                foreach (var (entity, component) in extraEntities)
                {
                    systemInput.Add((entity, new[] { component }, Array.Empty<string>()));
                }

            var ix = Bolt.World.ApplySystem(
                new PublicKey(WorldPda),
                new Bolt.System(BundleId, "test"),
                systemInput.ToArray(),
                args,
                authority,
                forceMainWalletSigner ? null : Web3Utils.SessionWallet?.SessionTokenPDA, BundleId, extraAccounts);

            Debug.Log($"Applying System {systemName} with args.. :  {JsonConvert.SerializeObject(args)}");
            return await ExecuteSystemApplicationInstruction(ix, forceMainWalletSigner);
        }

        public Bolt.Component GetComponent()
        {
            return new Bolt.Component(BundleId, GetComponentName());
        }


        private async UniTask<bool> ExecuteSystemApplicationInstruction(
            TransactionInstruction systemApplicationInstruction, bool signWithWallet)
        {
            var signerAccount = Web3Utils.SessionToken == null || signWithWallet
                ? Wallet.Account
                : Web3Utils.SessionWallet.Account;

            var signers = new List<Account> { signerAccount };

            var blockHashResponse = await RpcClient.GetLatestBlockHashAsync(Commitment.Processed);
            if (!blockHashResponse.WasSuccessful || blockHashResponse.Result?.Value?.Blockhash == null)
                throw new Exception("Failed to get latest blockhash");
            var blockhash = blockHashResponse.Result.Value.Blockhash;
            var transaction = new TransactionBuilder()
                .SetFeePayer(signerAccount)
                .SetRecentBlockHash(blockhash)
                .AddInstruction(systemApplicationInstruction)
                .AddInstruction(ComputeBudgetProgram.SetComputeUnitLimit(1000000)) //be generous for now
                .Build(signers);

            var sendTx = await RpcClient.SendTransactionAsync(transaction, true, Commitment.Confirmed);
            await RpcClient.ConfirmTransaction(sendTx.Result, Commitment.Confirmed);

            Debug.Log($"System Application Result: {sendTx.WasSuccessful} {sendTx.Result}");

            var DEBUG = true;
            if (DEBUG)
            {
                var tx = await RpcClient.GetTransactionAsync(sendTx.Result, Commitment.Confirmed);

                if (tx.Result?.Meta?.Error != null)
                {
                    var errorMessage =
                        $"Failed At: {RpcClient.NodeAddress.AbsoluteUri} \n{JsonConvert.SerializeObject(tx.Result.Meta.Error)}";

                    Debug.LogError(errorMessage);
                    return false;
                }
            }

            var signerWallet = Web3Utils.SessionToken == null || signWithWallet ? Wallet : Web3Utils.SessionWallet;
            Debug.Log($"Balance: {await signerWallet.GetBalance()}");

            return true;
        }

        public async UniTask<Transaction> DelegateTransaction(PublicKey entityPda, PublicKey playerDataPda)
        {
            
            var feePayer = Web3Utils.SessionToken == null
                ? Web3.Wallet.Account
                : Web3Utils.SessionWallet.Account;

            
            
            
            var tx = new Transaction()
            {
                FeePayer = feePayer,
                Instructions = new List<TransactionInstruction>(),
                RecentBlockHash = await Web3.BlockHash(commitment: Commitment.Confirmed, useCache: false)
            };
            // Increase compute unit limit
            tx.Instructions.Add(ComputeBudgetProgram.SetComputeUnitLimit(75000));
            tx.Instructions.Add(ComputeBudgetProgram.SetComputeUnitPrice(100000));

            var delegateComponent = await Bolt.World.DelegateComponent(feePayer, entityPda, new Component(BundleId, GetComponentName()));
            tx.Add(delegateComponent.Instruction);

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

            // Undelegate the player data pda
            tx.Add(GetUndelegateIx(playerDataPda));

            return tx;
        }

        protected abstract TransactionInstruction GetUndelegateIx(PublicKey playerDataPda);

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