using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View
{
    public class ZoomCameraController : InjectableBehaviour
    {
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private float zoomSpeedTouch = 0.1f;
        [SerializeField] private float zoomSpeedMouse = 0.5f;

        [SerializeField] private float[] zoomBounds = { 10f, 85f };

        private Camera _cam;
        private bool _wasZoomingLastFrame; // Touch mode only
        private Vector2[] _lastZoomPositions; // Touch mode only

        private void Start()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            if (eventSystem.IsPointerOverGameObject())
                return;

            if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
                HandleTouch();
            else
                HandleMouse();
        }

        private void HandleTouch()
        {
            switch (Input.touchCount)
            {
                case 2: // Zooming
                    var newPositions = new[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                    if (!_wasZoomingLastFrame)
                    {
                        _lastZoomPositions = newPositions;
                        _wasZoomingLastFrame = true;
                    }
                    else
                    {
                        // Zoom based on the distance between the new positions compared to the 
                        // distance between the previous positions.
                        var newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                        var oldDistance = Vector2.Distance(_lastZoomPositions[0], _lastZoomPositions[1]);
                        var offset = newDistance - oldDistance;

                        ZoomCamera(offset, zoomSpeedTouch);
                        _lastZoomPositions = newPositions;
                    }

                    break;

                default:
                    _wasZoomingLastFrame = false;
                    break;
            }
        }

        private void HandleMouse()
        {
            // Check for scrolling to zoom the camera
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            ZoomCamera(scroll, zoomSpeedMouse);
        }

        private void ZoomCamera(float offset, float speed)
        {
            if (offset == 0)
                return;

            _cam.orthographicSize = Mathf.Clamp(
                _cam.orthographicSize - (offset * speed * UnityEngine.Time.deltaTime * 60),
                zoomBounds[0], zoomBounds[1]);
        }
    }
}