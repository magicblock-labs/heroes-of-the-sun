using Connectors;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View.Exploration
{
    public class PointAndClickMovement : InjectableBehaviour
    {
        [Inject] private HeroConnector _connector;

        private float _mouseDownTime;
        // private Action<Vector2Int> _callback;


        public void SetDataAddress(string value)
        {
            //}, Action<Vector2Int> callback)
            {
                _connector.SetDataAddress(value);
                // _callback = callback;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                _mouseDownTime = Time.time;

            if (Input.GetMouseButtonUp(0) && Time.time - _mouseDownTime < .5f)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(mouseRay, out var info, 1000f)) return;

                var tile = info.collider.GetComponent<RenderTile>();
                _ = _connector.Move(tile.Location.x, tile.Location.y);
                // _callback?.Invoke(tile.Location);
            }
        }
    }
}