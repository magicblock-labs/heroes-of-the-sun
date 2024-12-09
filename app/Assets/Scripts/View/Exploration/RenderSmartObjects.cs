using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderSmartObjects : InjectableBehaviour
    {
        [Inject] private SmartObjectModel _model;
        [Inject] private PathfindingModel _pathfinding;

        [SerializeField] private GameObject prefab;

        private Dictionary<Vector2Int, Transform> _objMap = new();

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
                foreach (var (cellPos, lootTransform) in _objMap)
                    lootTransform.localPosition = new Vector3(lootTransform.localPosition.x,
                        _pathfinding.GetY(cellPos) + 2f, lootTransform.localPosition.z);

                yield return new WaitForSeconds(1);
            }
        }

        private async void Redraw()
        {
            _objMap.Clear();

            foreach (Transform child in transform)
                Destroy(child.gameObject);
            
            foreach (var pos2d in _model.Get())
            {
                var pos = ConfigModel.GetWorldCellPosition(pos2d.x, pos2d.y);
                pos.y = _pathfinding.GetY(pos2d) + 1f;

                var objTransform = Instantiate(prefab, transform).transform;
                objTransform.localPosition = pos;

                _objMap[pos2d] = objTransform;
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(Redraw);
        }
    }
}