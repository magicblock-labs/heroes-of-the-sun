//#define FTUE_TESTING

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Connectors;
using Model;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Injection;

namespace Utils
{
    public class Bootstrap : InjectableBehaviour
    {
        private const string PwdPrefKey = "pwd";
        [SerializeField] private TMP_Text label;

        [Inject] private PlayerConnector _player;
        [Inject] private TokenConnector _token;
        [Inject] private LocationAllocatorConnector _allocator;
        [Inject] private LootDistributionConnector _loot;
        [Inject] private PlayerSettlementConnector _settlement;
        [Inject] private HeroConnector _hero;


        [Inject] private PlayerModel _playerModel;
        [Inject] private SettlementModel _settlementModel;
        [Inject] private LootModel _lootModel;


        private IEnumerator Start()
        {
            yield return null;
            label.text = "Sign In..";

            DontDestroyOnLoad(gameObject);

            Login();
        }

        async void Login()
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
                        Login();
                    }
                }

                else
                {
                    var mnemonic = new Mnemonic(WordList.English, WordCount.TwentyFour).ToString().Trim();
                    password = RandomString(10);

                    PlayerPrefs.SetString(PwdPrefKey, password);

                    await Web3.Instance.CreateAccount(mnemonic, password);
                }
            }
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Range(0, s.Length)]).ToArray());
        }


        private async void HandleSignIn(Account account)
        {
            Debug.Log("HandleSignIn:");
            Debug.Log(account.PublicKey);

            Web3.OnLogin -= HandleSignIn;
            label.text = $"[{Web3.Account.PublicKey}] Balance top up.. ";

            await Web3Utils.EnsureBalance();
            label.text = $"[{Web3.Account.PublicKey}] Loading Player Data.. ";

            await _player.SetSeed(Web3.Account.PublicKey.Key[..20]);

            _playerModel.Set(await _player.LoadData());
            label.text = $"[{Web3.Account.PublicKey}] Loaded ";

            //check if settlement exists
            var settlements = _playerModel.Get().Settlements;
            if (settlements.Length == 0)
            {
                //otherwise - get state of allocator
                await _allocator.SetSeed(LocationAllocatorConnector.DefaultSeed);
                var allocator = await _allocator.LoadData();

                await _settlement.SetSeed($"{allocator.CurrentX}x{allocator.CurrentY}");

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
                await _settlement.SetSeed($"{settlements[0].X}x{settlements[0].Y}");

            //todo make connectors subscribe and dont keep bootstrap alive
            _settlementModel.Set(await _settlement.LoadData());


            //load loot
            await _loot.SetSeed(LootDistributionConnector.DefaultSeed);
            _lootModel.Set(await _loot.LoadData());
            await _loot.Subscribe((_, _, loot) => { _lootModel.Set(loot); });

            await _settlement.Subscribe((sub, _, settlement) => { _settlementModel.Set(settlement); });

            await _token.LoadData();
            await _token.Subscribe(null);

            //ensure hero is created
            await _hero.SetEntityPda(_player.EntityPda);
            var hero = await _hero.LoadData();
            if (hero.Owner == null || hero.Owner.ToString().All(c => c == '1'))
            {
                await _player.AssignHero(
                    new Dictionary<PublicKey, PublicKey>
                    {
                        { new PublicKey(_hero.EntityPda), _hero.GetComponentProgramAddress() },
                    });
            }

            //sync time
            await Web3Utils.SyncTime();
            SceneManager.LoadScene("World");
        }
    }
}