using Solana.Unity.SDK;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace View.UI
{
    public class HandleLogout : MonoBehaviour
    {
        public void OnLogout()
        {
            Web3.Wallet.Logout();
            SceneManager.LoadScene(0);
        }
    }
}
