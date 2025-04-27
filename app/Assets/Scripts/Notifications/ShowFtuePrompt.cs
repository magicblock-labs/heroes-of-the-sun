using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Notifications
{
    public struct FtuePrompt
    {
        public string promptText;
        public Rect cutoutScreenSpace;
        public Vector2Int promptLocation;
        public bool blocking;
    }
    
    [Singleton]
    public class ShowFtuePrompt : Signal<FtuePrompt[]>
    {
        
    }
}