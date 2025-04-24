using System;
using DG.Tweening;
using Model;
using Notifications;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View
{
    public class PanningCameraController : InjectableBehaviour
    {
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] FocusOn _focusOn;

        [SerializeField] private EventSystem eventSystem;

        private Camera _cam;
        private Vector3 _lastPanPosition;

        private int _panFingerId; // Touch mode only

        private void Start()
        {
            _cam = GetComponent<Camera>();
            _focusOn.Add(OnFocus);
        }

        private void OnFocus(Vector3 position)
        {
            var angles = transform.rotation.eulerAngles;
            var forwardOffset = transform.position.y * (float)Math.Sin((90 - angles.x) * Mathf.Deg2Rad);
            var cameraOffset = new Vector3(Mathf.Cos(angles.y * Mathf.Deg2Rad), 0, Mathf.Sin(angles.y * Mathf.Deg2Rad)) * forwardOffset;

            transform.DOMove(new Vector3(
                position.x + cameraOffset.x,
                transform.position.y,
                position.z + cameraOffset.z), .2f);
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
                case 1: // Panning
                    // If the touch began, capture its position and its finger ID.
                    // Otherwise, if the finger ID of the touch doesn't match, skip it.
                    var touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        _lastPanPosition = touch.position;
                        _panFingerId = touch.fingerId;
                    }
                    else if (touch.fingerId == _panFingerId && touch.phase == TouchPhase.Moved &&
                             _gridInteraction.State is GridInteractionState.Idle or GridInteractionState.Panning)
                    {
                        _gridInteraction.SetState(GridInteractionState.Panning);
                        PanCamera(touch.position);
                    }
                    else if (_gridInteraction.State == GridInteractionState.Panning)
                        _gridInteraction.SetState(GridInteractionState.Idle);

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
                _gridInteraction.State is GridInteractionState.Idle &&
                _lastPanPosition != Input.mousePosition)
            {
                _gridInteraction.SetState(GridInteractionState.Panning);
            }

            if (_gridInteraction.State == GridInteractionState.Panning)
            {
                PanCamera(Input.mousePosition);

                if (Input.GetMouseButtonUp(0)) _gridInteraction.SetState(GridInteractionState.Idle);
            }
        }

        private void PanCamera(Vector3 value)
        {
            var eulerAngles = transform.rotation.eulerAngles;
            var radRotation = eulerAngles * (float)Math.PI / 180f;


            //rewrite while we can resize (no need on device if no rotation enabled)
            var screenVector = new Vector2(Screen.width, Screen.height).normalized;

            var diff = _cam.ScreenToViewportPoint(_lastPanPosition - value) * screenVector *
                       (2 * _cam.orthographicSize * (1 / screenVector.y));

            // this math is a bit flaky, recheck
            var rotated = new Vector3(
                -diff.y * (float)Math.Sin(radRotation.x) + diff.x * (float)Math.Cos(radRotation.y),
                0,
                diff.y * (float)Math.Sin(radRotation.x) + diff.x * (float)Math.Cos(radRotation.y)
            );

            // Perform the movement
            transform.Translate(
                rotated,
                Space.World);

            // Cache the position
            _lastPanPosition = value;
        }

        private void OnDestroy()
        {
            _gridInteraction.SetState(GridInteractionState.Idle);
            _focusOn.Remove(OnFocus);
        }
    }
}