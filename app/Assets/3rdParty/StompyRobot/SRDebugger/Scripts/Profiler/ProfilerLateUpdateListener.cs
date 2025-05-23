﻿using System;
using UnityEngine;

namespace StompyRobot.SRDebugger.Scripts.Profiler
{
    /// <summary>
    /// The profiler has a separate monobehaviour to listen for LateUpdate, and is placed
    /// at the end of the script execution order.
    /// </summary>
    public class ProfilerLateUpdateListener : MonoBehaviour
    {
        public Action OnLateUpdate;

        private void LateUpdate()
        {
            if (OnLateUpdate != null)
            {
                OnLateUpdate();
            }
        }
    }
}
