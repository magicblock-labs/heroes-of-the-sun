// #define ENABLE_TEST_SROPTIONS

using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
#if !DISABLE_SRDEBUGGER
#endif

namespace StompyRobot.SROptions
{
    public partial class SROptions
    {
        // Uncomment the #define at the top of file to enable test options

#if ENABLE_TEST_SROPTIONS

        private static readonly Dictionary<string, GameObject> Cache = new();

        static GameObject Get(string name)
        {
            if (!Cache.ContainsKey(name))
                Cache[name] = GameObject.Find(name);
            return Cache[name];
        }

        [Category("EnableContainer")]
        public bool VFX
        {
            get => Get("VFX").activeSelf;
            set => Get("VFX").SetActive(value);
        }

        [Category("EnableContainer")]
        public bool Env
        {
            get => Get("Env").activeSelf;
            set => Get("Env").SetActive(value);
        }


        [Category("EnableContainer")]
        public bool Nav
        {
            get => Get("Nav").activeSelf;
            set => Get("Nav").SetActive(value);
        }


        [Category("EnableContainer")]
        public bool Lights
        {
            get => Get("Lights").activeSelf;
            set => Get("Lights").SetActive(value);
        }

#endif
    }
}