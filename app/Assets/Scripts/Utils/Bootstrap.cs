// #define FTUE_TESTING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
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
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject loaderContainer;
        [SerializeField] private Image loader;
        [SerializeField] private Graphic[] final;
        [SerializeField] private GameObject disclaimerPanel;

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

        private CancellationTokenSource _initCts;
        private CancellationTokenSource _lifecycleCts;
        private bool _signedIn;
        private bool _disclaimerAccepted;


        private void OnEnable()
        {
            _lifecycleCts = new CancellationTokenSource();
            ResetVisuals();
            RestartInitIfNotSignedIn();
        }

        private void OnDisable()
        {
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = null;

            CleanupBeforeRestart();
        }

        private void RestartInitIfNotSignedIn()
        {
            if (_signedIn) return;
            _initCts = new CancellationTokenSource();
            _ = InitAsync(_initCts.Token);
        }

        private void CleanupBeforeRestart()
        {
            _initCts?.Cancel();
            _initCts?.Dispose();
            _initCts = null;

            Web3.OnLogin -= HandleSignIn;

            StopAllCoroutines();
            ResetVisuals();
        }

        private void ResetVisuals()
        {
            if (disclaimerPanel != null) disclaimerPanel.SetActive(false);
            
            _progress = 0f;
            if (loader != null) loader.fillAmount = 0f;
            if (loaderContainer != null) loaderContainer.SetActive(false);
            if (label != null) label.text = "Sign In..";
            if (final != null)
            {
                foreach (var g in final)
                    if (g != null)
                        g.color = new Color(1, 1, 1, 0f);
            }
        }

        async Task InitAsync(CancellationToken ct)
        {
            if (_signedIn || ct.IsCancellationRequested) return;

            try
            {
                await InitialiseAnalytics();
                if (_signedIn || ct.IsCancellationRequested) return;

                Web3.OnLogin += HandleSignIn;

#if UNITY_EDITOR
                _ = LoginInGameWalletAsync();
#else
                _ = Web3.Instance.LoginWalletAdapter();
#endif
            }
            finally
            {
                if (ct.IsCancellationRequested)
                    Web3.OnLogin -= HandleSignIn;
            }
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
                    password = Web3Utils.RandomString(10);

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

        private void Update()
        {
            if (loader.fillAmount < _progress)
            {
                loader.fillAmount += Time.deltaTime * 0.5f;
            }
        }


        private async void HandleSignIn(Account account)
        {
            if (_signedIn) return;
            _signedIn = true;

            Debug.Log("HandleSignIn:");
            Debug.Log(account.PublicKey);

            Web3.OnLogin -= HandleSignIn;

            try
            {
                _initCts?.Cancel();
                _initCts?.Dispose();
                _initCts = null;
            }
            catch
            {
            }

            loaderContainer.SetActive(true);

            AnalyticsService.Instance.RecordEvent(new CustomEvent("SignIn")
            {
                { "PublicKey", account.PublicKey.ToString() },
            });

            Debug.Log("Initialize Session..");
            await CreateNewSession();

            _progress = .1f;

            var accountBump = PlayerPrefs.GetInt("ACC_BUMP", 0);
            label.text = $"[{Web3.Account.PublicKey}] Loading Player Data.. {accountBump}";
            await _player.SetSeed($"{accountBump}{Web3.Account.PublicKey.Key}"[..20]);
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
            //todo make connectors subscribe and don't keep bootstrap alive
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
            label.text = $"Creating Hero Data... {_player.EntityPda}";
            await _hero.SetEntityPda(_player.EntityPda, true, true); //set hero to public so others can interact with it
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
                if (hero.X == 0 && hero.Y == 0)
                    //initial position
                    await _hero.Move(settlement.X * 96 - 1, settlement.Y * 96 - 1);
                else
                    await _hero.Move(hero.X, hero.Y); //just clone to rollup
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
            if (Web3Utils.SessionWallet == null)
                await Web3Utils.RefreshSessionWallet();

            var sessionTokenData =
                (await Web3.Rpc.GetAccountInfoAsync(Web3Utils.SessionWallet.SessionTokenPDA, Commitment.Confirmed))
                .Result;

            if (sessionTokenData?.Value?.Data[0] == null)
                return null;

            var sessionToken = SessionToken.Deserialize(Convert.FromBase64String(sessionTokenData.Value.Data[0]));

            return sessionToken;
        }

        private static bool IsSessionValid(long buffer = 60 * 60) //make sure it's valid for at least 1h ahead
        {
            return Web3Utils.SessionValidUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds() + buffer;
        }

        public void DisclaimerAccept()
        {
            _disclaimerAccepted = true;
            if (disclaimerPanel != null) disclaimerPanel.SetActive(false);
        }
        
        public void DisclaimerExit()
        {
            Application.Quit();
        }
        
        private async Task CreateNewSession()
        {
            if (await UpdateSessionValid())
                return;

            if (!_disclaimerAccepted && disclaimerPanel != null)
            {
                disclaimerPanel.SetActive(true);
                await WaitForDisclaimerAccepted(_lifecycleCts != null ? _lifecycleCts.Token : CancellationToken.None);
            }

            if (Web3Utils.SessionToken != null)
                await Web3Utils.SessionWallet.CloseSession();

            var transaction = new Transaction
            {
                FeePayer = Web3.Account,
                Instructions = new List<TransactionInstruction>(),
                RecentBlockHash = await Web3.BlockHash(Commitment.Confirmed, false)
            };

            var sessionIx = Web3Utils.SessionWallet.CreateSessionIX(true, GetSessionKeysEndTime(), 100000000);
            transaction.Add(sessionIx);
            transaction.PartialSign(new[] { Web3.Account, Web3Utils.SessionWallet.Account });

            var res = await Web3.Wallet.SignAndSendTransaction(transaction, true, Commitment.Confirmed);

            Debug.Log("Create session wallet: " + res.RawRpcResponse);
            await Web3.Wallet.ActiveRpcClient.ConfirmTransaction(res.Result, Commitment.Confirmed);
            var sessionValid = await UpdateSessionValid();
            Debug.Log("After create session, the session is valid: " + sessionValid);
        }

        private long GetSessionKeysEndTime()
        {
            return DateTimeOffset.UtcNow.AddDays(6).ToUnixTimeSeconds();
        }

        private async Task WaitForDisclaimerAccepted(CancellationToken ct)
        {
            while (!_disclaimerAccepted && (ct == CancellationToken.None || !ct.IsCancellationRequested))
            {
                await Task.Yield();
            }
        }
    }
}