﻿using StompyRobot.SRF.Scripts.Components;

namespace StompyRobot.SRF.Scripts.Service
{
    public abstract class SRServiceBase<T> : SRMonoBehaviourEx where T : class
    {
        protected override void Awake()
        {
            base.Awake();
            SRServiceManager.RegisterService<T>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SRServiceManager.UnRegisterService<T>();
        }
    }
}
