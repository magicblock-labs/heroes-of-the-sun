using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Notifications
{
    [Singleton]
    public class FocusOn : Signal<Vector3>
    {
    }
}