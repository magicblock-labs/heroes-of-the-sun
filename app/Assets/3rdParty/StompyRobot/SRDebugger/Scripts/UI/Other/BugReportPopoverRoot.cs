using StompyRobot.SRF.Scripts.Components;
using UnityEngine;

namespace StompyRobot.SRDebugger.Scripts.UI.Other
{
    public class BugReportPopoverRoot : SRMonoBehaviourEx
    {
        [RequiredField] public CanvasGroup CanvasGroup;

        [RequiredField] public RectTransform Container;
    }
}
