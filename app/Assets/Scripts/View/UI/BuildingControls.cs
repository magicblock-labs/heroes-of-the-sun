using Model;
using Service;
using Settlement.Types;
using Unity.AI.Navigation;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class BuildingControls : InjectableBehaviour
    {
        [SerializeField] private BuildingProgress progress;
        [SerializeField] private BuildingInfo info;

        [Inject] private InteractionStateModel _interaction;
        [Inject] private ProgramConnector _connector;

        private Camera _camera;
        private BoxCollider _collider;
        private NavMeshModifierVolume _obstacle;
        
        public void SetData(int index, Building value, BuildingConfig config)
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
            var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
            var intersectRay = _collider.bounds.IntersectRay(mouseRay, out _);


            if (Input.GetMouseButtonUp(0))
                info.ShowExtendedControls(intersectRay && _interaction.State == InteractionState.Idle);
        }
    }
}