using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Notifications
{
    public class ResourceDiff
    {
        public int Food;
        public int Wood;
        public int Water;
        public int Stone;
    }
    
    [Singleton]
    public class ResourceDiffNotification:Signal<ResourceDiff, float, Transform>
    {
        
    }

}