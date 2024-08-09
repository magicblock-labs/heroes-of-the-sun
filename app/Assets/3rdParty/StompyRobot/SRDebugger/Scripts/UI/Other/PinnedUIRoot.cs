using StompyRobot.SRF.Scripts.Components;
using StompyRobot.SRF.Scripts.UI.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace StompyRobot.SRDebugger.Scripts.UI.Other
{
    public class PinnedUIRoot : SRMonoBehaviourEx
    {
        [RequiredField] public Canvas Canvas;

        [RequiredField] public RectTransform Container;

        [RequiredField] public DockConsoleController DockConsoleController;

        [RequiredField] public GameObject Options;

        [RequiredField] public FlowLayoutGroup OptionsLayoutGroup;

        [RequiredField] public GameObject Profiler;

        [RequiredField] public HandleManager ProfilerHandleManager;

        [RequiredField] public VerticalLayoutGroup ProfilerVerticalLayoutGroup;
    }
}
