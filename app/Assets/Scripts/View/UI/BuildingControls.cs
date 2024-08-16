using Model;
using Service;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using View;
using View.UI;

public class BuildingControls : InjectableBehaviour
{
    [SerializeField] private BuildingProgress progress;
    [SerializeField] private BuildingInfo info;
    [SerializeField] private DisplayLabourAllocation labourAllocation;


    [Inject] private InteractionStateModel _interaction;
    [Inject] private ProgramConnector _connector;

    private Camera _camera;
    private BoxCollider _collider;

    public void SetData(int index, Building value, BuildingConfig config)
    {
        _camera = Camera.main;
        _collider = gameObject.GetComponent<BoxCollider>();

        if (_collider == null)
            _collider = gameObject.AddComponent<BoxCollider>();

        _collider.size = new Vector3(config.width * 2, .1f, config.height * 2);

        progress.SetData(value, config);
        info.SetData(value, config);

        info.gameObject.SetActive(false);
        labourAllocation.SetBuildingIndex(index);
    }

    private void LateUpdate()
    {
        var mouseRay = _camera.ScreenPointToRay(Input.mousePosition);
        var intersectRay = _collider.bounds.IntersectRay(mouseRay, out _);


        if (Input.GetMouseButtonUp(0))
            info.gameObject.SetActive(intersectRay && _interaction.State == InteractionState.Idle);
    }
}