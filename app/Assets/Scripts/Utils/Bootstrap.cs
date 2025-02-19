// #define FTUE_TESTING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Connectors;
using Model;
using Newtonsoft.Json;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.SessionKeys.GplSession.Program;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using TMPro;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Injection;
using World.Program;
using Random = UnityEngine.Random;

namespace Utils
{
    public enum WalletType
    {
        None,
        Adapter,
        Web3Auth,
        InGame
    }

    public class Bootstrap : InjectableBehaviour
    {
        private const string PwdPrefKey = "pwd";
        private const string SessionPwdPrefKey = nameof(SessionPwdPrefKey);
        public const string SelectedWalletTypeKey = nameof(SelectedWalletTypeKey);
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject loginSelector;

        [Inject] private PlayerConnector _player;
        [Inject] private TokenConnector _token;
        [Inject] private LocationAllocatorConnector _allocator;
        [Inject] private LootDistributionConnector _loot;
        [Inject] private PlayerSettlementConnector _settlement;
        [Inject] private HeroConnector _hero;


        [Inject] private PlayerModel _playerModel;
        [Inject] private SettlementModel _settlementModel;
        [Inject] private LootModel _lootModel;
        private PublicKey _sessionToken;


        private IEnumerator Start()
        {
            loginSelector.SetActive(false);
            yield return null;

            label.text = "Sign In..";

            _ = InitAsync();
        }

        async Task InitAsync()
        {
            await InitialiseAnalytics();

            var type = (WalletType)PlayerPrefs.GetInt(SelectedWalletTypeKey, (int)WalletType.None);

            switch (type)
            {
                case WalletType.Adapter:
                    LoginWalletAdapter();
                    break;
                case WalletType.Web3Auth:
                    LoginWeb3Auth();
                    break;
                case WalletType.InGame:
                    await LoginInGameWalletAsync();
                    break;

                case WalletType.None:
                default:
                    loginSelector.SetActive(true);
                    break;
            }
        }

        public void LoginWalletAdapter()
        {
            PlayerPrefs.SetInt(SelectedWalletTypeKey, (int)WalletType.Adapter);
            _ = Web3.Instance.LoginWalletAdapter();
        }

        public void LoginWeb3Auth()
        {
            PlayerPrefs.SetInt(SelectedWalletTypeKey, (int)WalletType.Web3Auth);
            _ = Web3.Instance.LoginWeb3Auth(Provider.GOOGLE);
        }

        public void LoginInGameWallet()
        {
            PlayerPrefs.SetInt(SelectedWalletTypeKey, (int)WalletType.InGame);
            _ = LoginInGameWalletAsync();
        }

        public async Task LoginInGameWalletAsync()
        {
            if (Web3.Account == null)
            {
                Web3.OnLogin += HandleSignIn;

                string password = PlayerPrefs.GetString(PwdPrefKey, null);

#if FTUE_TESTING
                password = null;
#endif

                if (!string.IsNullOrEmpty(password))
                {
                    var account = await Web3.Instance.LoginInGameWallet(password);
                    if (account == null) //password corrupt - recreate
                    {
                        PlayerPrefs.DeleteAll();
                        _ = LoginInGameWalletAsync();
                    }
                    else
                    {
                        await Web3Utils.EphemeralWallet.Login(password);
                    }
                }

                else
                {
                    var mnemonic = new Mnemonic(WordList.English, WordCount.TwentyFour).ToString().Trim();
                    password = RandomString(10);

                    // // TODO: Remove this as it's for testing only
                    // var mnemonic = "wet mistake floor suffer melody talk tackle fame uncle inherit thing dumb jazz wolf smart lawsuit carbon denial found alert huge liar cost wealth";
                    // password = "12312738912739123";

                    PlayerPrefs.SetString(PwdPrefKey, password);
                    await Web3.Instance.CreateAccount(mnemonic, password);
                    await Web3Utils.EphemeralWallet.CreateAccount(mnemonic, password);
                }
            }
        }

        private async Task InitialiseAnalytics()
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName(Web3.Instance.rpcCluster.ToString().ToLower());

            await UnityServices.InitializeAsync(options);
            AnalyticsService.Instance.StartDataCollection();
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Range(0, s.Length)]).ToArray());
        }


        private async void HandleSignIn(Account account)
        {
            Web3.OnLogin -= HandleSignIn;

            Destroy(loginSelector);

            AnalyticsService.Instance.RecordEvent(new CustomEvent("SignIn")
            {
                { "PublicKey", account.PublicKey.ToString() },
            });

            Debug.Log("HandleSignIn:");
            Debug.Log(account.PublicKey);

            if (Web3.Wallet is not InGameWallet)
            {
                Debug.Log("Initialize Session..");
                await CreateSession();
            }
            else
            {
                label.text = $"[{Web3.Account.PublicKey}] Balance top up.. ";
                await Web3Utils.EnsureBalance();
            }

            label.text = $"[{Web3.Account.PublicKey}] Loading Player Data.. ";
            await _player.SetSeed(Web3.Account.PublicKey.Key[..20]);
            _playerModel.Set(await _player.LoadData());


            //check if settlement exists
            var settlements = _playerModel.Get().Settlements;
            if (settlements.Length == 0)
            {
                label.text = $"Fetching new location for Settlements.. ";
                //otherwise - get state of allocator
                await _allocator.SetSeed(LocationAllocatorConnector.DefaultSeed);
                var allocator = await _allocator.LoadData();

                label.text = $"Creating Settlementn at {allocator.CurrentX}_{allocator.CurrentY}...";
                await _settlement.SetSeed($"{allocator.CurrentX}_{allocator.CurrentY}");

                Debug.Log(JsonConvert.SerializeObject(await _settlement.LoadData()));

                // await _settlement.Undelegate();

                label.text = $"Assigning Settlement to the Player...";
                label.text = $"Assigning Settlement to the Player...";
                //assign settlement in player
                await _player.AssignSettlement(
                    new Dictionary<PublicKey, PublicKey>
                    {
                        { new PublicKey(_settlement.EntityPda), _settlement.GetComponentProgramAddress() },
                        { new PublicKey(_allocator.EntityPda), _allocator.GetComponentProgramAddress() },
                    });

                _playerModel.Set(await _player.LoadData());
            }
            else
                await _settlement.SetSeed($"{settlements[0].X}_{settlements[0].Y}");

            label.text = $"Loading Settlement Data...";
            //todo make connectors subscribe and dont keep bootstrap alive
            _settlementModel.Set(await _settlement.LoadData());

            if (await _settlement.Delegate())
                await _settlement.CloneToRollup();

            //load loot
            label.text = $"Loading Loot Data...";
            await _loot.SetSeed(LootDistributionConnector.DefaultSeed);
            _lootModel.Set(await _loot.LoadData());

            await _loot.Subscribe(_lootModel.Set);
            await _settlement.Subscribe(_settlementModel.Set);

            label.text = $"Init Gold Token...";
            await _token.LoadData();
            await _token.Subscribe(null);

            //ensure hero is created
            label.text = $"Creating Hero Data...";
            await _hero.SetEntityPda(_player.EntityPda);
            var hero = await _hero.LoadData();

            if (hero.Owner == null || hero.Owner.ToString().All(c => c == '1'))
            {
                label.text = $"Assigning New Hero to Player...";
                await _player.AssignHero(
                    new Dictionary<PublicKey, PublicKey>
                    {
                        { new PublicKey(_hero.EntityPda), _hero.GetComponentProgramAddress() },
                    });
            }


            label.text = $"Delegating Hero...";
            if (await _hero.Delegate())
            {
                var settlement = _playerModel.Get().Settlements[0];
                await _hero.Move(settlement.X * 48 - 1, settlement.Y * 48 - 1);
            }

            //sync time
            label.text = $"SyncTime...";
            await Web3Utils.SyncTime();
            label.text = $"Load Settlement...";
            SceneManager.LoadScene("Settlement");
        }

        private async Task CreateSession()
        {
            async Task CreateSessionSigner()
            {
                var mnemonic = new Mnemonic(WordList.English, WordCount.TwentyFour).ToString().Trim();
                var password = RandomString(10);

                PlayerPrefs.SetString(SessionPwdPrefKey, password);
            
                await Web3Utils.SessionWallet.CreateAccount(mnemonic, password);
            }

            var password = PlayerPrefs.GetString(SessionPwdPrefKey, null);

            if (!string.IsNullOrEmpty(password))
            {
                var account = await Web3.Instance.LoginInGameWallet(password);
                if (account == null) //password corrupt - recreate
                {
                    PlayerPrefs.DeleteKey(SessionPwdPrefKey);
                    await CreateSessionSigner();
                }
                else
                {
                    await Web3Utils.SessionWallet.Login(password);
                }
            }

            else
            {
                await CreateSessionSigner();
            }
        
            Web3Utils.SessionToken =
                WorldProgram.FindSessionTokenPda(Web3Utils.SessionWallet.Account.PublicKey, Web3.Wallet.Account.PublicKey);
            
            var createSession = new CreateSessionAccounts()
            {
                SessionToken = _sessionToken,
                SessionSigner = Web3Utils.SessionWallet.Account.PublicKey,
                Authority = Web3.Wallet.Account.PublicKey,
                TargetProgram = new PublicKey(WorldProgram.ID)
            };

            var latestBlockHash =
                await Web3.Wallet.ActiveRpcClient.GetLatestBlockHashAsync(commitment: Commitment.Processed);
            var tx = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>
                {
                    GplSessionProgram.CreateSession(createSession, true, 1000, new(WorldProgram.ID))
                },
                RecentBlockHash = latestBlockHash.Result.Value.Blockhash
            };

            var signedTx = await Web3.Wallet.SignTransaction(tx);

            var signature = await Web3.Wallet.ActiveRpcClient.SendTransactionAsync(
                Convert.ToBase64String(signedTx.Serialize()),
                skipPreflight: true, preFlightCommitment: Commitment.Confirmed);


            if (signature.WasSuccessful)
                Debug.Log(signature.Result);

            var errorMessage = signature.Reason;
            errorMessage += "\n" + signature.RawRpcResponse;
            if (signature.ErrorData != null)
            {
                errorMessage += "\n" + string.Join("\n", signature.ErrorData.Logs);
            }

            Debug.LogError(errorMessage);
        }
    }
}