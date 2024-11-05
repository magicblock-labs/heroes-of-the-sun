using Model;
using Unity.AI.Navigation;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class RenderGroundTiles : InjectableBehaviour, IDisplaySettlementData
    {
        [Inject] ConfigModel _config;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject occupiedTilePrefab;
        [SerializeField] private GameObject surroundingTile;
        [SerializeField] private float scaleFactor = 0.95f;

        public void SetData(Settlement.Accounts.Settlement value)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var occupiedData = _config.GetCellsData(value);

            for (var i = -2; i < occupiedData.GetLength(0) + 2; i++)
            for (var j = -2; j < occupiedData.GetLength(1) + 2; j++)
            {
                GameObject prefab;

                if (i < 0 || j < 0 || i >= occupiedData.GetLength(0) ||
                    j >= occupiedData.GetLength(1))
                    prefab = surroundingTile;
                else if (occupiedData[i, j] == 0)
                    prefab = tilePrefab;
                else
                    prefab = occupiedTilePrefab;

                CreatePrefab(prefab, i, j);
            }

            var navMeshSurface = GetComponent<NavMeshSurface>();
            if (navMeshSurface)
                navMeshSurface.BuildNavMesh();
        }


        private void CreatePrefab(GameObject prefab, int i, int j)
        {
            var obj = Instantiate(prefab, transform, true);

            obj.transform.localPosition = ConfigModel.GetWorldCellPosition(i, j);
            obj.transform.localScale *= ConfigModel.CellSize * scaleFactor;
        }
    }
}