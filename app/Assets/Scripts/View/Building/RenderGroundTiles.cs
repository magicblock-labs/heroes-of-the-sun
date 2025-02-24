using System.Collections.Generic;
using Model;
using Unity.AI.Navigation;
using UnityEngine;
using Utils.Injection;

namespace View.Building
{
    public class RenderGroundTiles : InjectableBehaviour, IDisplaySettlementData
    {
        [Inject] ConfigModel _config;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject occupiedTilePrefab;
        [SerializeField] private GameObject surroundingTile;
        [SerializeField] private float scaleFactor = 0.95f;

        private byte[,] _renderedData;
        private Dictionary<Vector2Int, GameObject> _renderedTiles = new();

        public void SetData(global::Settlement.Accounts.Settlement value)
        {
            var occupiedData = _config.GetCellsData(value);
            var rebuildNavmesh = false;

            for (var i = -2; i < occupiedData.GetLength(0) + 2; i++)
            for (var j = -2; j < occupiedData.GetLength(1) + 2; j++)
            {
                var posVector = new Vector2Int(i, j);

                var isSurroundingTile =
                    i < 0 || j < 0 || i >= occupiedData.GetLength(0) || j >= occupiedData.GetLength(1);

                if (isSurroundingTile)
                {
                    if (!_renderedTiles.ContainsKey(posVector))
                        _renderedTiles[posVector] = CreatePrefab(surroundingTile, i, j);
                }
                else
                {
                    if (_renderedData != null && _renderedData[i, j] == occupiedData[i, j])
                        continue;

                    rebuildNavmesh = true;

                    if (_renderedTiles.TryGetValue(posVector, out var tile))
                        Destroy(tile);

                    _renderedTiles[posVector] =
                        CreatePrefab(occupiedData[i, j] == 0 ? tilePrefab : occupiedTilePrefab, i, j);
                }
            }

            var navMeshSurface = GetComponent<NavMeshSurface>();
            if (navMeshSurface != null && rebuildNavmesh)
                navMeshSurface.BuildNavMesh();

            _renderedData = occupiedData;
        }


        private GameObject CreatePrefab(GameObject prefab, int i, int j)
        {
            var obj = Instantiate(prefab, transform, true);

            obj.transform.localPosition = ConfigModel.GetWorldCellPosition(i, j);
            obj.transform.localScale *= ConfigModel.CellSize * scaleFactor;

            return obj;
        }
    }
}