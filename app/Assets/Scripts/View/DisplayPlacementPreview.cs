using System.Collections;
using Model;
using Service;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class DisplayPlacementPreview : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interaction;
        [Inject] private BuildingsModel _buildings;
        [Inject] private ConfigModel _config;

        [SerializeField] private GameObject gridElement;

        [SerializeField] private Material cell;
        [SerializeField] private Material blocked;
        [SerializeField] private Material available;


        public const int CellSize = 2; //units per cell

        [SerializeField] private BuildingPreview previewPrefab;
        private BuildingPreview _preview;
        private GameObject[,] _cells;
        private BuildingConfig _selectedBuildingConfig;

        private IEnumerator Start()
        {
            _interaction.Updated.Add(OnModeUpdated);
            yield return new WaitForSeconds(1);
        }

        private void OnModeUpdated()
        {
            if (_interaction.SelectedBuildingType != BuildingType.None)
            {
                LazyInit();
                
                _selectedBuildingConfig = _config.Buildings[_interaction.SelectedBuildingType];
                _preview.SetBuildingPrefab(_selectedBuildingConfig);
                
                if (!_preview.gameObject.activeSelf){
                    _preview.gameObject.SetActive(true);
                    
                    
                    var buildingDimensions =
                        new Vector3(_selectedBuildingConfig.width, 0, _selectedBuildingConfig.height);
                    
                    ApplyWorldPoint(new Vector3(_config.Width/2 * CellSize, 0, _config.Height/2 * CellSize), buildingDimensions);
                }
            }
            else
                Reset();
        }

        private void LazyInit()
        {
            if (_cells == null)
                _cells = new GameObject[_buildings.OccupiedData.GetLength(0), _buildings.OccupiedData.GetLength(1)];

            for (var i = 0; i < _buildings.OccupiedData.GetLength(0); i++)
            for (var j = 0; j < _buildings.OccupiedData.GetLength(1); j++)
            {
                if (_cells[i, j] == null)
                {
                    var obj = Instantiate(gridElement, transform, true);
                    obj.transform.localPosition = new Vector3((i + 0.5f) * CellSize, 0.5f, (j + 0.5f) * CellSize);
                    obj.transform.localScale *= CellSize;
                    _cells[i, j] = obj;
                }
                else
                    _cells[i, j].SetActive(true);
            }
            
            

            if (_preview == null){
                _preview = Instantiate(previewPrefab, transform);
                _preview.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_interaction.SelectedBuildingType != BuildingType.None)
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                
                var buildingDimensions =
                    new Vector3(_selectedBuildingConfig.width, 0, _selectedBuildingConfig.height);
                
                if (_interaction.State == InteractionState.Dragging)
                {
                    
                    if (!transform.GetComponent<Collider>().bounds.IntersectRay(mouseRay, out var distance)) return;

                    var intersectionPoint = mouseRay.origin + mouseRay.direction * distance;
                    var worldPoint = transform.InverseTransformPoint(intersectionPoint);

                    ApplyWorldPoint(worldPoint, buildingDimensions);
                }

                
                

                _preview.transform.localPosition =
                    (new Vector3(_interaction.CellPosX, 0, _interaction.CellPosZ) + buildingDimensions / 2) * CellSize;
            }
        }

        private void ApplyWorldPoint(Vector3 worldPoint, Vector3 buildingDimensions)
        {

            var cellPos = worldPoint / CellSize - buildingDimensions / 2;

            var cellPosX = Mathf.RoundToInt(cellPos.x);
            var cellPosZ = Mathf.RoundToInt(cellPos.z);

            _interaction.SetPlacementLocation(
                cellPosX,
                cellPosZ,
                buildingDimensions,
                _buildings.OccupiedData);

            var previewMaterial = _interaction.ValidPlacement ? available : blocked;

            if (!_preview.gameObject.activeSelf) //maybe lets add an explicit flag
                return;

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
                            : _buildings.OccupiedData[i, j] == 0
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
            _interaction.Updated.Remove(OnModeUpdated);
        }
    }
}