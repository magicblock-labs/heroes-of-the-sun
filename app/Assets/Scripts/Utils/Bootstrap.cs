// #define FTUE_TESTING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Connectors;
using GplSession.Accounts;
using Model;
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
using Debug = UnityEngine.Debug;
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
        private const string PwdPrefKey = nameof(PwdPrefKey);
        private const string SessionPwdPrefKey = nameof(SessionPwdPrefKey);
        public const string SelectedWalletTypeKey = nameof(SelectedWalletTypeKey);
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject loginSelector;
        [SerializeField] private GameObject loaderContainer;
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
        private PublicKey _sessionToken;


        private IEnumerator Start()
        {
            loginSelector.SetActive(false);
            loaderContainer.SetActive(false);
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
                    // ReSharper disable once MethodHasAsyncOverload
                    //its not an async overload lol
                    
                    LoginInGameWallet();
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
            Web3.OnLogin += HandleSignIn;
            _ = Web3.Instance.LoginWalletAdapter();
        }

        public void LoginWeb3Auth()
        {
            PlayerPrefs.SetInt(SelectedWalletTypeKey, (int)WalletType.Web3Auth);
            Web3.OnLogin += HandleSignIn;
            _ = Web3.Instance.LoginWeb3Auth(Provider.GOOGLE);
        }

        public void LoginInGameWallet()
        {
            PlayerPrefs.SetInt(SelectedWalletTypeKey, (int)WalletType.InGame);
            Web3.OnLogin += HandleSignIn;
            _ = LoginInGameWalletAsync();
        }

        public async Task LoginInGameWalletAsync()
        {
            if (Web3.Account == null)
            {
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
            Debug.Log("HandleSignIn:");
            Debug.Log(account.PublicKey);
            
            Web3.OnLogin -= HandleSignIn;

            Destroy(loginSelector);
            loaderContainer.SetActive(true);

            AnalyticsService.Instance.RecordEvent(new CustomEvent("SignIn")
            {
                { "PublicKey", account.PublicKey.ToString() },
            });

            if (true) //(Web3.Wallet is not InGameWallet)
            {
                Debug.Log("Initialize Session..");
                await CreateNewSession();
            }
            // else
            // {
            //     label.text = $"[{Web3.Account.PublicKey}] Balance top up.. ";
            //     await Web3Utils.EnsureBalance();
            // }


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


                _progress = .3f;

                label.text = $"Creating new Settlement...";

                await _settlement.SetSeed(await _allocator.GetNextUnallocatedLocation());
                await _settlement.LoadData();

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
            for (var i = 0f; i < 1f; i += Time.deltaTime)
            {
                foreach (var g in final)
                    g.color = new Color(1, 1, 1, Mathf.Lerp(0f, 1f, i));
                yield return null;
            }

            SceneManager.LoadScene("Settlement");
        }

        // public async Task RevokeSession()
        // {
        //     await Web3Utils.SessionWallet.CloseSession();
        //     Debug.Log("Session closed");
        // }
        //
        // public async Task<bool> IsSessionTokenInitialized()
        // {
        //     var sessionTokenData =
        //         await Web3.Rpc.GetAccountInfoAsync(Web3Utils.SessionWallet.SessionTokenPDA, Commitment.Confirmed);
        //     if (sessionTokenData.Result != null && sessionTokenData.Result.Value != null)
        //     {
        //         return true;
        //     }
        //
        //     return false;
        // }

        public async Task<bool> UpdateSessionValid()
        {
            Web3Utils.SessionToken = await RequestSessionToken();

            if (Web3Utils.SessionToken == null) return false;

            Debug.Log("Session token valid until: " +
                      (new DateTime(1970, 1, 1)).AddSeconds(Web3Utils.SessionToken.ValidUntil) +
                      " Now: " + DateTimeOffset.UtcNow);
            Web3Utils.SessionValidUntil = Web3Utils.SessionToken.ValidUntil;
            return IsSessionValid();
        }

        public async Task<SessionToken> RequestSessionToken()
        {
            //nullify if expired TODO
            if (Web3Utils.SessionWallet == null)
                await RefreshSessionWallet();

            var sessionTokenData =
                (await Web3.Rpc.GetAccountInfoAsync(Web3Utils.SessionWallet.SessionTokenPDA, Commitment.Confirmed))
                .Result;

            if (sessionTokenData?.Value?.Data[0] == null)
                return null;

            var sessionToken = SessionToken.Deserialize(Convert.FromBase64String(sessionTokenData.Value.Data[0]));

            return sessionToken;
        }

        private bool IsSessionValid()
        {
            return Web3Utils.SessionValidUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private async Task RefreshSessionWallet()
        {
            var password = PlayerPrefs.GetString(SessionPwdPrefKey, null);

            if (string.IsNullOrEmpty(password))
            {
                password = RandomString(10);
                PlayerPrefs.SetString(SessionPwdPrefKey, password);
            }

            Web3Utils.SessionWallet = await SessionWallet.GetSessionWallet(new PublicKey(WorldProgram.ID),
                password,
                Web3.Wallet);
        }

        private async Task CreateNewSession()
        {
            Web3Utils.SessionToken = await RequestSessionToken();
            if (Web3Utils.SessionToken != null)
            {
                await Web3Utils.SessionWallet.CloseSession();
            }

            
            SessionWallet.Instance = null;
            await RefreshSessionWallet();
            
            var transaction = new Transaction()
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>(),
                RecentBlockHash = await Web3.BlockHash(Commitment.Confirmed, false)
            };

            var sessionIx = Web3Utils.SessionWallet.CreateSessionIX(true, GetSessionKeysEndTime(), 100000000);
            transaction.Add(sessionIx);
            transaction.PartialSign(new[] { Web3.Account, Web3Utils.SessionWallet.Account });

            var res = await Web3.Wallet.SignAndSendTransaction(transaction, true,  Commitment.Confirmed);

            Debug.Log("Create session wallet: " + res.RawRpcResponse);
            await Web3.Wallet.ActiveRpcClient.ConfirmTransaction(res.Result, Commitment.Confirmed);
            var sessionValid = await UpdateSessionValid();
            Debug.Log("After create session, the session is valid: " + sessionValid);
        }

        private long GetSessionKeysEndTime()
        {
            return DateTimeOffset.UtcNow.AddDays(6).ToUnixTimeSeconds();
        }
    }
}