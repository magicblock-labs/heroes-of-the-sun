using System;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class CameraController : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private float zoomSpeedTouch = 0.1f;
        [SerializeField] private float zoomSpeedMouse = 0.5f;

        [SerializeField] private float[] zoomBounds = { 10f, 85f };

        private Camera _cam;
        private Vector3 _lastPanPosition;

        private int _panFingerId; // Touch mode only
        private bool _wasZoomingLastFrame; // Touch mode only
        private Vector2[] _lastZoomPositions; // Touch mode only


        private void Start()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            if (_interaction.State == InteractionState.Dialog)
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
                case 1: // Panning
                    _wasZoomingLastFrame = false;

                    // If the touch began, capture its position and its finger ID.
                    // Otherwise, if the finger ID of the touch doesn't match, skip it.
                    var touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        _lastPanPosition = touch.position;
                        _panFingerId = touch.fingerId;
                    }
                    else if (touch.fingerId == _panFingerId && touch.phase == TouchPhase.Moved &&
                             _interaction.State is InteractionState.Idle or InteractionState.Panning)
                    {
                        _interaction.SetState(InteractionState.Panning);
                        PanCamera(touch.position);
                    }
                    else if (_interaction.State == InteractionState.Panning)
                        _interaction.SetState(InteractionState.Idle);

                    break;

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
            // On mouse down, capture it's position.
            // Otherwise, if the mouse is still down, pan the camera.
            if (Input.GetMouseButtonDown(0))
                _lastPanPosition = Input.mousePosition;

            if (
                Input.GetMouseButton(0) &&
                _interaction.State is InteractionState.Idle &&
                _lastPanPosition != Input.mousePosition)
            {
                _interaction.SetState(InteractionState.Panning);
            }

            if (_interaction.State == InteractionState.Panning)
            {
                PanCamera(Input.mousePosition);

                if (Input.GetMouseButtonUp(0)) _interaction.SetState(InteractionState.Idle);
            }

            // Check for scrolling to zoom the camera
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            ZoomCamera(scroll, zoomSpeedMouse);
        }

        private void PanCamera(Vector3 value)
        {
            
            var eulerAngles = transform.rotation.eulerAngles;
            var radRotation = eulerAngles * (float)Math.PI / 180f;

            
            //rewrite while we can resize (no need on device if no rotation enabled)
            var screenVector = new Vector2(Screen.width, Screen.height).normalized;
            
            var diff = _cam.ScreenToViewportPoint(_lastPanPosition - value) * screenVector * (2 * _cam.orthographicSize * (1/screenVector.y));
            
            // var rotated = new Vector3(
            //     diff.x - diff.y * (float)Math.Sin(radRotation.y), 
            //     0,
            //     diff.y / (float)Math.Cos(radRotation.x) - diff.x * (float)Math.Cos(radRotation.y)
            // );
            
            var rotated = new Vector3(
                -diff.y * (float)Math.Sin(radRotation.x) + diff.x, 
                0,
                diff.y * (float)Math.Sin(radRotation.x) + diff.x
            );

            // Perform the movement
            transform.Translate(
                rotated,
                Space.World);

            // Cache the position
            _lastPanPosition = value;
        }

        private void ZoomCamera(float offset, float speed)
        {
            if (offset == 0)
                return;

            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize - (offset * speed * UnityEngine.Time.deltaTime * 60),
                zoomBounds[0], zoomBounds[1]);
        }
    }
}