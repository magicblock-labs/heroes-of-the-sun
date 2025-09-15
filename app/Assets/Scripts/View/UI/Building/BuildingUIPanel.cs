using UnityEngine;
using UnityEngine.Rendering;
using Utils.Injection;

namespace View.UI.Building
{
    public abstract class AnchoredUIPanel : InjectableBehaviour
    {
        [SerializeField] public Transform worldAnchor;

        private Camera _camera;

        protected Camera Camera
        {
            get
            {
                if (_camera == null)
                    _camera = Camera.main;

                return _camera;
            }
        }

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

            if (canvas != null)
                canvas.worldCamera = Camera;
        }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera _)
        {
            if (worldAnchor == null)
                return;

            ApplyPos(worldAnchor.position);
        }

        protected void ApplyPos(Vector3 value)
        {
            transform.position = value + Camera.transform.forward * -10;
            transform.localScale = Vector3.one * 25f / Camera.orthographicSize;
        }
    }
}