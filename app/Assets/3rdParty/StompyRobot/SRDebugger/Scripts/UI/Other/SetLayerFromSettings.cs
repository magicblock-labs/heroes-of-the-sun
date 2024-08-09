using StompyRobot.SRF.Scripts.Components;
using StompyRobot.SRF.Scripts.Extensions;

namespace StompyRobot.SRDebugger.Scripts.UI.Other
{
    public class SetLayerFromSettings : SRMonoBehaviour
    {
        private void Start()
        {
            gameObject.SetLayerRecursive(Settings.Instance.DebugLayer);
        }
    }
}
