﻿using System;
using UnityEngine.Scripting;

namespace StompyRobot.SRF.Scripts.Service
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceAttribute : PreserveAttribute
    {
        public ServiceAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ServiceSelectorAttribute : PreserveAttribute
    {
        public ServiceSelectorAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ServiceConstructorAttribute : PreserveAttribute
    {
        public ServiceConstructorAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        public Type ServiceType { get; private set; }
    }
}
