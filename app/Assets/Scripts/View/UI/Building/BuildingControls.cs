using Connectors;
using Model;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View.UI.Building
{
    public class BuildingControls : InjectableBehaviour
    {
        [SerializeField] private BuildingProgress progress;
        [SerializeField] private BuildingInfo info;

        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private PlayerSettlementConnector _connector;

        private Camera _camera;
        private BoxCollider _collider;
        private NavMeshModifierVolume _obstacle;

        public void SetData(int index, Settlement.Types.Building value, BuildingConfig config)
        {
            _camera = Camera.main;

            _collider = gameObject.GetComponent<BoxCollider>();
            _collider.size = new Vector3(config.width * 2, .1f, config.height * 2);

            _obstacle = gameObject.GetComponent<NavMeshModifierVolume>();
            _obstacle.size = (new Vector3(_collider.size.x, 2, _collider.size.z));

            progress.SetData(value, config);
            info.SetData(index, value);
        }

        private void LateUpdate()
        {
            if (!Input.GetMouseButtonUp(0)) return;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (!_gridInteraction.CanInteract)
                return;

            var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
            var intersectRay = _collider.bounds.IntersectRay(mouseRay, out _);

            info.ShowControls(
                intersectRay
                && _gridInteraction.State == GridInteractionState.Idle
                && !_gridInteraction.SelectedBuildingType.HasValue);
        }
    }
}