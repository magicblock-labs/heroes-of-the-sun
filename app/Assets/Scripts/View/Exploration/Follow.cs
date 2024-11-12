using UnityEngine;

namespace View.Exploration
{
    public class Follow : MonoBehaviour
    {
        [SerializeField] private Vector3 offset;

        private Transform _target;
        private float _rot;
        private float _rotDuringMouseDown;
        private float _mousePositionOnDown;

        void Update()
        {
            if (_target == null)
            {
                var hero = FindFirstObjectByType<PointAndClickMovement>();
                if (hero!=null)
                    _target = hero.transform;
            }
            
            if (_target == null)
            {
                return;
            }
            
            var currentTargetPosition = _target.position;
            var targetPoint = currentTargetPosition + Quaternion.Euler(0, _rot, 0) * offset;

            if (Input.GetMouseButtonDown(0))
            {
                _rotDuringMouseDown = _rot;
                _mousePositionOnDown = Input.mousePosition.x;
            }

            if (Input.GetMouseButton(0))
            {
                _rot = _rotDuringMouseDown + (Input.mousePosition.x - _mousePositionOnDown) / 4;
                transform.position = targetPoint;
            }
            else
            {
                transform.position += (targetPoint - transform.position) * Time.deltaTime;
            }

            transform.LookAt(currentTargetPosition, Vector3.up);
        }
    }
}