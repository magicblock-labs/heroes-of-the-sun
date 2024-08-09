using System;
using StompyRobot.SRF.Scripts.Components;
using StompyRobot.SRF.Scripts.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace StompyRobot.SRDebugger.Scripts.UI.Controls.Data
{
    public class ActionControl : OptionsControlBase
    {
        private MethodReference _method;

        [RequiredField] public UnityEngine.UI.Button Button;

        [RequiredField] public Text Title;

        public MethodReference Method
        {
            get { return _method; }
        }

        protected override void Start()
        {
            base.Start();
            Button.onClick.AddListener(ButtonOnClick);
        }

        private void ButtonOnClick()
        {
            if (_method == null)
            {
                Debug.LogWarning("[SRDebugger.Options] No method set for action control", this);
                return;
            }

            try
            {
                _method.Invoke(null);
            }
            catch (Exception e)
            {
                Debug.LogError("[SRDebugger] Exception thrown while executing action.");
                Debug.LogException(e);
            }
        }

        public void SetMethod(string methodName, MethodReference method)
        {
            _method = method;
            Title.text = methodName;
        }
    }
}
