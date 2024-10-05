using System;
using UnityEngine;
using UnityEngine.Rendering;
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

            var parent = transform;
            while (parent != null)
            {
                canvas = parent.GetComponent<Canvas>();
                if (canvas != null)
                    break;

                parent = transform.parent;
            }

            if (_camera == null)
                _camera = Camera.main;

            if (canvas != null)
                canvas.worldCamera = _camera;
        }
        
        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            
            if (worldAnchor == null)
                return;

            transform.position = worldAnchor.position + _camera.transform.forward * -7;
            transform.localScale = Vector3.one * 25f / _camera.orthographicSize;
        }
    }
}