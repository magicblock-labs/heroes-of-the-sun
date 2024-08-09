// #define FTUE_TESTING

using System.Threading.Tasks;
using Service;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Injection;

namespace Utils
{
    public class Bootstrap : InjectableBehaviour
    {
        [SerializeField] private TMP_Text label;

        [Inject] private RemoteConnectorMock _connectorMock;
        [Inject] private BuilderService _service;

        private void Start()
        {
            label.text = "Sign In..";
            _connectorMock.SignIn();
            HandleSignIn();
        }


        private async void HandleSignIn()
        {
            label.text = $"[{_connectorMock.AccountId}] Loading Player Data.. ";

#if FTUE_TESTING
            await _connector.ResetAccounts();
#endif

            if (!await _service.ReloadData())
            {
                label.text = $"[{_connectorMock.AccountId}] Creating A New Player..";

                await _service.Initialise();

                Debug.Log("Initialised!");

                label.text = $"[{_connectorMock.AccountId}] New Player Created..";

                await _service.ReloadData();
            }
            
            
            label.text = $"[{_connectorMock.AccountId}] Loaded ";
        }

        public static async Task DelayAsync(float secondsDelay)
        {
            var startTime = UnityEngine.Time.time;
            while (UnityEngine.Time.time < startTime + secondsDelay) await Task.Yield();
        }
    }
}