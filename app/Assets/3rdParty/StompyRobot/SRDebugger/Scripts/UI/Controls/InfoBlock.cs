using StompyRobot.SRF.Scripts.Components;
using UnityEngine.UI;

namespace StompyRobot.SRDebugger.Scripts.UI.Controls
{
    public class InfoBlock : SRMonoBehaviourEx
    {
        [RequiredField] public Text Content;

        [RequiredField] public Text Title;
    }
}
