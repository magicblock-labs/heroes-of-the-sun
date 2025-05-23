﻿using System;
using StompyRobot.SRF.Scripts.Components;
using UnityEngine.UI;

namespace StompyRobot.SRDebugger.Scripts.UI.Controls.Data
{
    public class ReadOnlyControl : DataBoundControl
    {
        [RequiredField]
        public Text ValueText;

        [RequiredField]
        public Text Title;

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnBind(string propertyName, Type t)
        {
            base.OnBind(propertyName, t);
            Title.text = propertyName;
        }

        protected override void OnValueUpdated(object newValue)
        {
            ValueText.text = Convert.ToString(newValue);
        }

        public override bool CanBind(Type type, bool isReadOnly)
        {
            return type == typeof(string) && isReadOnly;
        }
    }
}
