using System.Collections;
using UnityEngine;

namespace StompyRobot.SRF.Scripts
{
    public static class Coroutines
    {
        public static IEnumerator WaitForSecondsRealTime(float time)
        {
            var endTime = Time.realtimeSinceStartup + time;

            while (Time.realtimeSinceStartup < endTime)
            {
                yield return null;
            }
        }
    }
}
