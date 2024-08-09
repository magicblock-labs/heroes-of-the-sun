using Service;
using UnityEngine.SceneManagement;
using Utils.Injection;

namespace View
{
    public class RequestReset : InjectableBehaviour
    {
        [Inject] private RemoteConnectorMock _connectorMock;

        public async void Reset()
        {
            await _connectorMock.ResetAccounts();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
