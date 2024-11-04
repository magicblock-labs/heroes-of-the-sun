using Model;
using Unity.AI.Navigation;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class RenderGroundTiles : InjectableBehaviour
    {
        [Inject] private SettlementModel _settlement;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject occupiedTilePrefab;
        [SerializeField] private GameObject surroundingTile;
        [SerializeField] private float scaleFactor = 0.95f;

        private void Start()
        {
            _settlement.Updated.Add(RedrawTiles);
        }

        void RedrawTiles()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            for (var i = -2; i < _settlement.OccupiedData.GetLength(0) + 2; i++)
            for (var j = -2; j < _settlement.OccupiedData.GetLength(1) + 2; j++)
            {
                GameObject prefab;

                if (i < 0 || j < 0 || i >= _settlement.OccupiedData.GetLength(0) ||
                    j >= _settlement.OccupiedData.GetLength(1))
                    prefab = surroundingTile;
                else if (_settlement.OccupiedData[i, j] == 0)
                    prefab = tilePrefab;
                else
                    prefab = occupiedTilePrefab;

                CreatePrefab(prefab, i, j);
            }
            
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        private void CreatePrefab(GameObject prefab, int i, int j)
        {
            var obj = Instantiate(prefab, transform, true);

            obj.transform.localPosition = ConfigModel.GetWorldCellPosition(i, j);
            obj.transform.localScale *= ConfigModel.CellSize * scaleFactor;
        }

        private void OnDestroy()
        {
            _settlement.Updated.Remove(RedrawTiles);
        }
    }
}