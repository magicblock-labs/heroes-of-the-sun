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
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using TMPro;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
        [SerializeField] private Image loader;
        [SerializeField] private Graphic[] final;

        [Inject] private PlayerConnector _player;
        [Inject] private TokenConnector _token;
        [Inject] private LocationAllocatorConnector _allocator;
        [Inject] private LootDistributionConnector _loot;
        [Inject] private PlayerSettlementConnector _settlement;
        [Inject] private HeroConnector _hero;

        [Inject] private PlayerModel _playerModel;
        [Inject] private SettlementModel _settlementModel;
        [Inject] private LootModel _lootModel;
        private float _progress;


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

        private void Update()
        {
            if (loader.fillAmount < _progress)
            {
                loader.fillAmount += Time.deltaTime * 0.5f;
            }
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
                await InitializeSession();
            }
            else
            {
                label.text = $"[{Web3.Account.PublicKey}] Balance top up.. ";
                await Web3Utils.EnsureBalance();
            }
            
            

            _progress = .1f;

            label.text = $"[{Web3.Account.PublicKey}] Loading Player Data.. ";
            await _player.SetSeed(Web3.Account.PublicKey.Key[..20]);
            _playerModel.Set(await _player.LoadData());
            
            _progress = .2f;
            
            //check if settlement exists
            var settlements = _playerModel.Get().Settlements;
            if (settlements.Length == 0)
            {
                label.text = $"Fetching new location for Settlements.. ";
                //otherwise - get state of allocator
                await _allocator.SetSeed(LocationAllocatorConnector.DefaultSeed);
                var allocator = await _allocator.LoadData();
                
                
                _progress = .3f;

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
                
                
                _progress = .4f;

                _playerModel.Set(await _player.LoadData());
            }
            else
                await _settlement.SetSeed($"{settlements[0].X}_{settlements[0].Y}");
            
            
            _progress = .5f;

            label.text = $"Loading Settlement Data...";
            //todo make connectors subscribe and dont keep bootstrap alive
            _settlementModel.Set(await _settlement.LoadData());

            if (await _settlement.Delegate())
                await _settlement.CloneToRollup();
            
            
            _progress = .6f;

            //load loot
            label.text = $"Loading Loot Data...";
            await _loot.SetSeed(LootDistributionConnector.DefaultSeed);
            _lootModel.Set(await _loot.LoadData());

            
            _progress = .7f;
            
            await _loot.Subscribe(_lootModel.Set);
            await _settlement.Subscribe(_settlementModel.Set);

            label.text = $"Init Gold Token...";
            await _token.LoadData();
            await _token.Subscribe(null);
            
            
            _progress = .8f;

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

            
            _progress = .9f;

            label.text = $"Delegating Hero...";
            if (await _hero.Delegate())
            {
                var settlement = _playerModel.Get().Settlements[0];
                await _hero.Move(settlement.X * 96 - 1, settlement.Y * 96 - 1);
            }


            _progress = 1;
            //sync time
            label.text = $"SyncTime...";
            await Web3Utils.SyncTime();
            label.text = $"Load Settlement...";

            StartCoroutine(LoadingCompleted());
        }

        private IEnumerator LoadingCompleted()
        {
            for (var i = 0f; i < 1f; i += Time.deltaTime){
                foreach (var g in final)
                    g.color = new Color(1, 1, 1,  Mathf.Lerp(0f, 1f, i));
                yield return null;
            }
            
            // SceneManager.LoadScene("Settlement");
        }

        private async Task InitializeSession()
        {
            var sessionPassword = PlayerPrefs.GetString(SessionPwdPrefKey, null);

            if (sessionPassword == null)
            {
                sessionPassword = RandomString(10);
                PlayerPrefs.SetString(SessionPwdPrefKey, sessionPassword);
            }

            Web3Utils.SessionWallet =
                await SessionWallet.GetSessionWallet(new PublicKey(WorldProgram.ID), sessionPassword);

            if (!await Web3Utils.SessionWallet.IsSessionTokenInitialized())
            {
                var tx = new Transaction
                {
                    FeePayer = Web3.Account,
                    Instructions = new List<TransactionInstruction>(),
                    RecentBlockHash = await Web3.BlockHash()
                };

                // Set to 1 day in unix time
                var validity = DateTimeOffset.UtcNow.AddHours(23).ToUnixTimeSeconds();
                tx.Instructions.Add(Web3Utils.SessionWallet.CreateSessionIX(true, validity));
                tx.PartialSign(new[] { Web3.Account, Web3Utils.SessionWallet.Account });

                await Web3.Wallet.ActiveRpcClient.SendTransactionAsync(Convert.ToBase64String(tx.Serialize()),
                    skipPreflight: true, preFlightCommitment: Commitment.Confirmed);
            }
        }
    }
}