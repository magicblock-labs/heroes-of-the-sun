using StompyRobot.SRDebugger.Scripts.UI.Controls;
using StompyRobot.SRF.Scripts.Components;
using StompyRobot.SRF.Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace StompyRobot.SRDebugger.Scripts.UI.Other
{
    public class TriggerRoot : SRMonoBehaviourEx
    {
        [RequiredField] public Canvas Canvas;

        [RequiredField] public LongPressButton TapHoldButton;

        [RequiredField] public RectTransform TriggerTransform;

        [RequiredField] public ErrorNotifier ErrorNotifier;

        [RequiredField] [FormerlySerializedAs("TriggerButton")] public MultiTapButton TripleTapButton;
    }
}
