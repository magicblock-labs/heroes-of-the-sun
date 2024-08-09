using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class BuildingUI : InjectableBehaviour
    {
        [SerializeField] private Transform worldAnchor;
        private Camera _camera;

        private void LateUpdate()
        {
            if (worldAnchor == null)
                return;
            
            if (_camera == null)
                _camera = Camera.main;

            var screenPoint = _camera.WorldToScreenPoint(worldAnchor.position);
            transform.position = screenPoint;
        }
    }
}