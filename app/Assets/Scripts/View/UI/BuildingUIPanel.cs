using System;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public abstract class BuildingUIPanel : InjectableBehaviour
    {
        [SerializeField] protected Transform worldAnchor;
        private Camera _camera;

        protected virtual void Start()
        {
            Canvas canvas = null;

            var p = transform;
            while (p != null)
            {
                canvas = p.GetComponent<Canvas>();
                if (canvas != null)
                    break;

                p = transform.parent;
            }

            if (_camera == null)
                _camera = Camera.main;

            if (canvas != null)
                canvas.worldCamera = _camera;
        }

        private void Update()
        {
            if (worldAnchor == null)
                return;

            transform.position = worldAnchor.position + _camera.transform.forward * -5;
            transform.localScale = Vector3.one * 25f / _camera.orthographicSize;
        }
    }
}