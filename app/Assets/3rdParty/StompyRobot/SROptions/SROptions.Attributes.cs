using System;

namespace StompyRobot.SROptions
{
    public partial class SROptions
    {
        // For compatibility with older versions of SRDebugger, this simply inherits from the component model version.

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
        public sealed class DisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
        {
            public DisplayNameAttribute(string displayName) : base(displayName)
            {
            }
        }

        // These attributes are used when using SROptions. Options added via SRDebug.Instance.AddOptionsContainer can use the attribute defined in SRDebugger namespace.

        [AttributeUsage(AttributeTargets.Property)]
        public sealed class IncrementAttribute :
#if DISABLE_SRDEBUGGER
        Attribute
#else
            SRDebugger.Scripts.IncrementAttribute
#endif
        {
            public IncrementAttribute(double increment)
#if !DISABLE_SRDEBUGGER
                : base(increment)
#endif
            {
            }
        }

        [AttributeUsage(AttributeTargets.Property)]
        public sealed class NumberRangeAttribute :
#if DISABLE_SRDEBUGGER
        Attribute
#else
            SRDebugger.Scripts.NumberRangeAttribute
#endif
        {
            public NumberRangeAttribute(double min, double max)
#if !DISABLE_SRDEBUGGER
                : base(min, max)
#endif
            {
            }
        }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
        public sealed class SortAttribute :
#if DISABLE_SRDEBUGGER
        Attribute
#else
            SRDebugger.Scripts.SortAttribute
#endif
        {
            public SortAttribute(int priority)
#if !DISABLE_SRDEBUGGER
                : base(priority)
#endif
            {
            }
        }
    }
}