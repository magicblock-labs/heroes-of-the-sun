using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Notifications
{
    [Singleton]
    public class PlayAudioClip : Signal<AudioClip>
    {
    }
}