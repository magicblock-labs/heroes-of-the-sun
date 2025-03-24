using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderLoot : InjectableBehaviour
    {
        private const int ChunkSize = 24;
        [Inject] private LootModel _model;
        [Inject] private PathfindingModel _pathfinding;

        [SerializeField] private GameObject prefab;

        private Dictionary<Vector2Int, Transform> _lootMap = new();

        private void Start()
        {
            _model.Updated.Add(Redraw);
            Redraw();

            StartCoroutine(UpdateYPositions());
        }

        private IEnumerator UpdateYPositions()
        {
            while (true)
            {
                //this is because _pathfinding gets populated upon world generation. potentially could be statically calculated (currently exact values come from chunk view) 
                foreach (var (cellPos, lootTransform) in _lootMap)
                    lootTransform.localPosition = new Vector3(lootTransform.localPosition.x,
                        _pathfinding.GetY(cellPos) + 2f, lootTransform.localPosition.z);

                yield return new WaitForSeconds(1);
            }
        }

        private async void Redraw()
        {

            _lootMap.Clear();

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var i = 0;
            foreach (var loot in _model.Get().Loots)
            {
                i++;
                var lootLocation = new Vector2Int(loot.X, loot.Y);


                var pos = ConfigModel.GetWorldCellPosition(lootLocation.x, lootLocation.y);
                pos.y = _pathfinding.GetY(new Vector2Int(lootLocation.x, lootLocation.y)) + 1f;

                var lootTransform = Instantiate(prefab, transform).transform;
                lootTransform.localPosition = pos;

                _lootMap[new Vector2Int(lootLocation.x, lootLocation.y)] = lootTransform;
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(Redraw);
        }
    }
}