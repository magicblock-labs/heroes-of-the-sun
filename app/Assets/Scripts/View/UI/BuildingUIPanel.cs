using Service;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public abstract class BuildingUIPanel : InjectableBehaviour
    {
        [SerializeField] protected Transform worldAnchor;
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