// #define FTUE_TESTING

using System.Collections;
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
        [Inject] private LocationAllocatorConnector _allocator;
        [Inject] private SettlementConnector _settlement;


        [Inject] private PlayerModel _playerModel;


        private IEnumerator Start()
        {
            yield return null;
            label.text = "Sign In..";

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
                    var mnemonic = new Mnemonic(WordList.English, WordCount.Twelve);
                    password = RandomString(10);

                    PlayerPrefs.SetString(PwdPrefKey, password);

                    await Web3.Instance.CreateAccount(mnemonic.ToString(), password);
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

            await _player.EnsureBalance();
            label.text = $"[{Web3.Account.PublicKey}] Loading Player Data.. ";

            await _player.ReloadData();
            label.text = $"[{Web3.Account.PublicKey}] Loaded ";

            //check if settlement exists
            var settlements = _playerModel.Get().Settlements;
            if (settlements.Length == 0)
            {
                //otherwise - get state of allocator
                var allocator = await _allocator.GetCurrentState();
                
                //assign settlement in player
                //todo this should be one system instruction with settlement account passed
                await _player.AssignSettlement(allocator.CurrentX, allocator.CurrentY);
                await _allocator.Bump();
                _settlement.Location = new Vector2Int(allocator.CurrentX, allocator.CurrentY);
            }
            else 
                _settlement.Location = new Vector2Int(settlements[0].X, settlements[0].Y);

            await _settlement.ReloadData();

            //sync time
            await _settlement.SyncTime();

            SceneManager.LoadScene("World");
        }
    }
}