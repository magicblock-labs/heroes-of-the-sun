using Connectors;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View.Exploration
{
    public class PointAndClickMovement : InjectableBehaviour
    {
        [Inject] private HeroConnector _connector;
        [Inject] private SmartObjectModel _smartObjects;
        [Inject] private InteractionStateModel _interaction;

        private float _mouseDownTime;

        public void SetDataAddress(string value)
        {
            _connector.SetDataAddress(value);
        }

        private void Update()
        {
            if (_interaction.State != InteractionState.Idle)
                return;
            
            if (Input.GetMouseButtonDown(0))
                _mouseDownTime = Time.time;

            if (Input.GetMouseButtonUp(0) && Time.time - _mouseDownTime < .5f)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(mouseRay, out var info, 1000f)) return;

                var tile = info.collider.GetComponent<RenderTile>();
                var tileLocation = tile.Location;
                
                if (_smartObjects.HasSmartObjectAt(tileLocation, out _))
                    tileLocation += Vector2Int.up;
                
                _ = _connector.Move(tileLocation.x, tileLocation.y);
            }
        }
    }
}