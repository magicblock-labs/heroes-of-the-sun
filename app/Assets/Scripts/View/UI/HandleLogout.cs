using Solana.Unity.SDK;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace View.UI
{
    public class HandleLogout : MonoBehaviour
    {
        public void OnLogout()
        {
            PlayerPrefs.SetInt(Utils.Bootstrap.SelectedWalletTypeKey, (int)Utils.WalletType.None);
            Web3.Wallet.Logout();
            SceneManager.LoadScene(0);
        }
    }
}
