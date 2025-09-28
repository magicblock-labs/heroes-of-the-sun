using System.Collections.Generic;
using Model;
using Unity.AI.Navigation;
using UnityEngine;
using Utils;
using Utils.Injection;

namespace View.Building
{
    public class RenderGroundTilesOnGPU : InjectableBehaviour, IDisplaySettlementData
    {
        [Header("Tile meshes")] public Mesh[] tileMeshes;

        [Header("Shared material (Enable GPU Instancing = ON)")]
        public Material instancedMat;

        [Inject] ConfigModel _config;

        [SerializeField] private float scaleFactor = 0.95f;

        private MeshDrawer _drawer = new();

        public void SetData(Settlement.Accounts.Settlement value, Vector2Int offset)
        {
            var tileMap = new Dictionary<int, List<Vector3>>();
            var occupiedData = _config.GetCellsData(value);

            for (var i = -2; i < occupiedData.GetLength(0) + 2; i++)
            for (var j = -2; j < occupiedData.GetLength(1) + 2; j++)
            {
                var isSurroundingTile =
                    i < 0 || j < 0 || i >= occupiedData.GetLength(0) || j >= occupiedData.GetLength(1);

                var tileIndex = isSurroundingTile
                    ? 2
                    : occupiedData[i, j] == 0
                        ? 0
                        : 1;


                if (!tileMap.ContainsKey(tileIndex))
                    tileMap.Add(tileIndex, new List<Vector3>());

                tileMap[tileIndex].Add(ConfigModel.GetWorldCellPosition(i + 2 + offset.x, j + 2 + offset.y));
            }

            _drawer.Init(tileMeshes, instancedMat);

            _drawer.BuildChunksFromTileMap(tileMap);
            _drawer.UpdateBatchData();
        }

        private void Update()
        {
            _drawer.UpdateBatchData();
            _drawer.Render(2 * scaleFactor);
        }
    }
}