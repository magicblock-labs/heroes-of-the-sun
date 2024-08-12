// #define FTUE_TESTING

using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Service;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
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

        [Inject] private ProgramConnector _connector;

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
                
                    var password = PlayerPrefs.GetString(PwdPrefKey, null);

                    if (!string.IsNullOrEmpty(password)){
                        var account = await  Web3.Instance.LoginInGameWallet(password);
                        if (account == null) //password corrupt - recreate
                        {
                            PlayerPrefs.DeleteAll();
                            Login();
                        }else 
                            HandleSignIn(account);   
                    }
                    
                    else
                    {
                        var mnemonic = new Mnemonic(WordList.English, WordCount.Twelve);
                        password = RandomString(10);

                        PlayerPrefs.SetString(PwdPrefKey, password);

                        var account = await Web3.Instance.CreateAccount(mnemonic.ToString(), password);
                        HandleSignIn(account);   
                    }
                }
                // else
                //     HandleSignIn(Web3.Account);            
            }
    
        

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
        }


        private async void HandleSignIn(Account account)
        {
            Web3.OnLogin -= HandleSignIn;

            label.text = $"[{Web3.Account.PublicKey}] Top up.. ";
            await EnsureBalance();

            label.text = $"[{Web3.Account.PublicKey}] Loading Player Data.. ";

#if FTUE_TESTING
            await _connector.ResetAccounts();
#endif

            await _connector.Initialise();
            return;
            if (!await _connector.ReloadData())
            {
                label.text = $"[{Web3.Account.PublicKey}] Creating A New Player..";

                await _connector.Initialise();

                Debug.Log("Initialised!");

                label.text = $"[{Web3.Account.PublicKey}] New Player Created..";

                await _connector.ReloadData();
            }


            label.text = $"[{Web3.Account.PublicKey}] Loaded ";
        }
        
        public async Task EnsureBalance()
        {
            if ((await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey)).Result.Value < 500000000)
                await Web3.Rpc.RequestAirdropAsync(Web3.Account.PublicKey, 1000000000);
        }
    }
}