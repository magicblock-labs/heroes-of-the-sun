using System.Collections;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Building
{
    [RequireComponent(typeof(BoxCollider))]
    public class DisplayPlacementPreview : InjectableBehaviour
    {
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private SettlementModel _settlement;
        [Inject] private ConfigModel _config;

        [SerializeField] private GameObject gridElement;

        [SerializeField] private Material cell;
        [SerializeField] private Material blocked;
        [SerializeField] private Material available;

        [SerializeField] private BuildingPreview previewPrefab;
        private BuildingPreview _preview;
        private GameObject[,] _cells;
        private BuildingConfig _selectedBuildingConfig;
        private BoxCollider _collider;

        private IEnumerator Start()
        {
            _collider = GetComponent<BoxCollider>();
            _collider.size = new Vector3(_config.Width, 1, _config.Height) * ConfigModel.CellSize;
            _collider.center = _collider.size / 2;
            _gridInteraction.Updated.Add(OnModeUpdated);
            yield return new WaitForSeconds(1);
        }

        private void OnModeUpdated()
        {
            if (_gridInteraction.SelectedBuildingType.HasValue)
            {
                LazyInit();

                _selectedBuildingConfig = _config.Buildings[_gridInteraction.SelectedBuildingType.Value];
                _preview.SetBuildingPrefab(_selectedBuildingConfig);

                if (!_preview.gameObject.activeSelf)
                {
                    _preview.gameObject.SetActive(true);

                    var buildingDimensions =
                        new Vector3(_selectedBuildingConfig.width, 0, _selectedBuildingConfig.height);

                    var initialPos = _settlement.GetUnoccupiedPositionFor(buildingDimensions);

                    ApplyCellPoint(initialPos.x, initialPos.z, buildingDimensions);
                }
            }
            else
                Reset();
        }

        private void LazyInit()
        {
            if (_cells == null)
                _cells = new GameObject[_settlement.OccupiedData.GetLength(0), _settlement.OccupiedData.GetLength(1)];

            for (var i = 0; i < _settlement.OccupiedData.GetLength(0); i++)
            for (var j = 0; j < _settlement.OccupiedData.GetLength(1); j++)
            {
                if (_cells[i, j] == null)
                {
                    var obj = Instantiate(gridElement, transform, true);
                    obj.transform.localPosition = new Vector3(
                        (i + 0.5f) * ConfigModel.CellSize, .15f,
                        (j + 0.5f) * ConfigModel.CellSize);
                    obj.transform.localScale *= ConfigModel.CellSize;
                    _cells[i, j] = obj;
                }
                else
                    _cells[i, j].SetActive(true);
            }

            if (_preview == null)
            {
                _preview = Instantiate(previewPrefab, transform);
                _preview.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_gridInteraction.SelectedBuildingType.HasValue)
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (_selectedBuildingConfig == null)
                    _selectedBuildingConfig = _config.Buildings[_gridInteraction.SelectedBuildingType.Value];

                var buildingDimensions =
                    new Vector3(_selectedBuildingConfig.width, 0, _selectedBuildingConfig.height);

                if (_gridInteraction.State == GridInteractionState.Dragging)
                {
                    if (!_collider.bounds.IntersectRay(mouseRay, out var distance)) return;

                    var intersectionPoint = mouseRay.origin + mouseRay.direction * distance;
                    var worldPoint = transform.InverseTransformPoint(intersectionPoint);

                    ApplyWorldPoint(worldPoint, buildingDimensions);
                }

                _preview.transform.localPosition =
                    (new Vector3(_gridInteraction.CellPosX, 0, _gridInteraction.CellPosZ) + buildingDimensions / 2) *
                    ConfigModel.CellSize;
            }
        }

        private void ApplyWorldPoint(Vector3 worldPoint, Vector3 buildingDimensions)
        {
            var cellPos = worldPoint / ConfigModel.CellSize - buildingDimensions / 2;

            var cellPosX = Mathf.RoundToInt(cellPos.x);
            var cellPosZ = Mathf.RoundToInt(cellPos.z);

            ApplyCellPoint(cellPosX, cellPosZ, buildingDimensions);
        }


        private void ApplyCellPoint(int cellPosX, int cellPosZ, Vector3 buildingDimensions)
        {
            _gridInteraction.SetPlacementLocation(
                cellPosX,
                cellPosZ,
                _settlement.IsValidLocation(cellPosX, cellPosZ, buildingDimensions));

            var previewMaterial = _gridInteraction.ValidPlacement ? available : blocked;

            if (!_preview.gameObject.activeSelf) //maybe lets add an explicit flag
                return;

            _preview.SetMaterialOverride(_gridInteraction.ValidPlacement ? cell : blocked);

            for (var i = 0; i < _cells.GetLength(0); i++)
            for (var j = 0; j < _cells.GetLength(1); j++)
            {
                var cellBelongsToBuilding =
                    i >= cellPosX &&
                    j >= cellPosZ &&
                    i < cellPosX + buildingDimensions.x &&
                    j < cellPosZ + buildingDimensions.z;
                {
                    _cells[i, j].GetComponent<MeshRenderer>().material =
                        cellBelongsToBuilding
                            ? previewMaterial
                            : _settlement.OccupiedData[i, j] == 0
                                ? cell
                                : blocked;
                }
            }
        }


        void Reset()
        {
            if (_preview != null)
                _preview.gameObject.SetActive(false);

            if (_cells != null)
                for (var i = 0; i < _cells.GetLength(0); i++)
                for (var j = 0; j < _cells.GetLength(1); j++)
                {
                    _cells[i, j].SetActive(false);
                }
        }

        private void OnDestroy()
        {
            _gridInteraction.Updated.Remove(OnModeUpdated);
        }
    }
}