using UnityEngine;

namespace View
{
    public class LinkSpeedToAnimator : MonoBehaviour
    {
        public Animator anim;
        private Vector3 _previousPos;
        private static readonly int Speed = Animator.StringToHash("Speed");

        // Update is called once per frame
        void Update()
        {
            var realSpeed = 0f;
            if (_previousPos != default)
            {
                var delta = transform.localPosition - _previousPos;
                delta.y = 0;
                realSpeed = delta.magnitude / UnityEngine.Time.deltaTime;
            }

            _previousPos = transform.localPosition;

            anim.SetFloat(Speed, realSpeed);
        }
    }
}