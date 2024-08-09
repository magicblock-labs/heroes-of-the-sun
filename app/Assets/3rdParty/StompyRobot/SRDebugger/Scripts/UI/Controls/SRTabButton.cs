using StompyRobot.SRF.Scripts.Components;
using StompyRobot.SRF.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StompyRobot.SRDebugger.Scripts.UI.Controls
{
    public class SRTabButton : SRMonoBehaviourEx
    {
        [RequiredField] public Behaviour ActiveToggle;

        [RequiredField] public UnityEngine.UI.Button Button;

        [RequiredField] public RectTransform ExtraContentContainer;

        [RequiredField] public StyleComponent IconStyleComponent;

        [RequiredField] public Text TitleText;

        public bool IsActive
        {
            get { return ActiveToggle.enabled; }
            set { ActiveToggle.enabled = value; }
        }
    }
}
