using System;
using UnityEngine;

namespace View
{
    [RequireComponent(typeof(Animator))]
    public class LinkSpeedToAnimator : MonoBehaviour
    {
        private Animator _anim;
        private Vector3 _previousPos;
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Start()
        {
            _anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            var realSpeed = 0f;
            if (_previousPos != default)
            {
                var delta = transform.localPosition - _previousPos;
                delta.y = 0;
                realSpeed = delta.magnitude / Time.deltaTime;
            }

            _previousPos = transform.localPosition;

            _anim.SetFloat(Speed, realSpeed);
        }
    }
}