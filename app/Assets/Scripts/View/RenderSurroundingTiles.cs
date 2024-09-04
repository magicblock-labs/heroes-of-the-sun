using System;
using Model;
using Service;
using UnityEngine;
using Utils.Injection;
using Random = UnityEngine.Random;

namespace View
{
    public class RenderSurroundingTiles : InjectableBehaviour
    {
        [Inject] private SettlementModel _settlement;

        [SerializeField] private GameObject[] tilePrefabs;
        [SerializeField] private float scaleFactor = 0.95f;
        [SerializeField] private int range = 4;
        [SerializeField] private int step = 2;

        private void Start()
        {
            _settlement.Updated.Add(RedrawTiles);
        }

        void RedrawTiles()
        {
            if (transform.childCount > 0)
                return;
            
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            for (var i = -range; i < _settlement.OccupiedData.GetLength(0) + range; i += step)
            for (var j = -range; j < _settlement.OccupiedData.GetLength(1) + range; j += step)
            {
                if (i >= 0 && i < _settlement.OccupiedData.GetLength(0)-1 && j >= 0 &&
                    j < _settlement.OccupiedData.GetLength(1)-1)
                    continue;

                var obj = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Length)], transform, true);
                obj.transform.localPosition = new Vector3(
                    (i + 1f) * ConfigModel.CellSize, 0,
                    (j + 1f) * ConfigModel.CellSize);
                obj.transform.localScale *= ConfigModel.CellSize * scaleFactor;
            }
        }

        private void OnDestroy()
        {
            _settlement.Updated.Remove(RedrawTiles);
        }
    }
}