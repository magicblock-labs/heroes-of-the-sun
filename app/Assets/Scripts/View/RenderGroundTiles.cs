using System;
using Model;
using Service;
using UnityEngine;
using Utils.Injection;
using Random = UnityEngine.Random;

namespace View
{
    public class RenderGroundTiles : InjectableBehaviour
    {
        [Inject] private SettlementModel _settlement;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject[] bushPrefabs;
        [SerializeField] private GameObject occupiedTilePrefab;
        [SerializeField] private float scaleFactor = 0.95f;

        private void Start()
        {
            _settlement.Updated.Add(RedrawTiles);
        }

        void RedrawTiles()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            for (var i = 0; i < _settlement.OccupiedData.GetLength(0); i++)
            for (var j = 0; j < _settlement.OccupiedData.GetLength(1); j++)
            {
                var isFree = _settlement.OccupiedData[i, j] == 0;

                CreatePrefab(isFree
                    ? tilePrefab
                    : occupiedTilePrefab, i, j);

                if (isFree && Random.value < 0.08f)
                    CreatePrefab(bushPrefabs[Random.Range(0, bushPrefabs.Length)], i, j);
            }
        }

        private void CreatePrefab(GameObject prefab, int i, int j)
        {
            var obj = Instantiate(prefab, transform, true);

            obj.transform.localPosition = new Vector3(
                (i + 0.5f) * ConfigModel.CellSize, 0,
                (j + 0.5f) * ConfigModel.CellSize);
            obj.transform.localScale *= ConfigModel.CellSize * scaleFactor;
        }

        private void OnDestroy()
        {
            _settlement.Updated.Remove(RedrawTiles);
        }
    }
}