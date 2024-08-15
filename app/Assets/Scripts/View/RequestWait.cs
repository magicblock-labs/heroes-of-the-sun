using Service;
using UnityEngine.SceneManagement;
using Utils.Injection;

namespace View
{
    public class RequestWait : InjectableBehaviour
    {
        [Inject] private ProgramConnector _connector;

        public async void Wait()
        {
            if (await _connector.Wait(1))
                await _connector.ReloadData();
        }
    }
}
