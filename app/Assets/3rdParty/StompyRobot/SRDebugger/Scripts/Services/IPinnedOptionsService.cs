using System;
using StompyRobot.SRDebugger.Scripts.Internal;
using UnityEngine;

namespace StompyRobot.SRDebugger.Scripts.Services
{
    public interface IPinnedUIService
    {
        event Action<OptionDefinition, bool> OptionPinStateChanged;
        event Action<RectTransform> OptionsCanvasCreated;

        bool IsProfilerPinned { get; set; }
        void Pin(OptionDefinition option, int order = -1);
        void Unpin(OptionDefinition option);
        void UnpinAll();
        bool HasPinned(OptionDefinition option);
    }
}
