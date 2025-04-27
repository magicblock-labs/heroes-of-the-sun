using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Notifications
{
    [Singleton]
    public class StartFtueSequence : Signal<QuestData>
    {
    }
}