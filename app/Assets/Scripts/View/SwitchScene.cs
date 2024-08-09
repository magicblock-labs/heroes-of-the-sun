using UnityEngine;
using UnityEngine.SceneManagement;

namespace View
{
    public class SwitchScene : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        public void Switch()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
