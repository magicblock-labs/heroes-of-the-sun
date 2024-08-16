// #define FTUE_TESTING

using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Service;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Wallet.Bip39;
using TMPro;
using UnityEngine;
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
                .Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
        }


        private async void HandleSignIn(Account account)
        {
            Web3.OnLogin -= HandleSignIn;

            label.text = $"[{Web3.Account.PublicKey}] Top up.. ";
            await EnsureBalance();

            label.text = $"[{Web3.Account.PublicKey}] Loading Player Data.. ";


            // await _connector.Initialise();
            await _connector.ReloadData();
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
            var requestResult = (await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey));
            if (requestResult.Result.Value < 500000000)
            {
                var airdropResult = await Web3.Rpc.RequestAirdropAsync(Web3.Account.PublicKey, 1000000000);
                Debug.Log(JsonConvert.SerializeObject(airdropResult));
                await Task.Delay(5000);
            }

            requestResult = (await Web3.Rpc.GetBalanceAsync(Web3.Account.PublicKey));

            Debug.Log(JsonConvert.SerializeObject(requestResult));
        }
    }
}