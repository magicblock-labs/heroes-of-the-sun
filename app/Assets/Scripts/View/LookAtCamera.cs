using UnityEngine;

namespace View
{
    public class LookAtCamera : MonoBehaviour
    {
        private Transform _target;
        [SerializeField] private Vector3 offset = new(0, 4, 0);

        void Start()
        {
            _target = Camera.main.transform;
        }

        void Update()
        {
            transform.position = transform.parent.position + offset;
            transform.forward = _target.forward;
        }
    }
}