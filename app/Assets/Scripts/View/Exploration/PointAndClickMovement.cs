using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View.Exploration
{
    public class PointAndClickMovement : InjectableBehaviour
    {
        [Inject] private PathfindingModel _pathfinding;
        [Inject] private HeroModel _hero;

        private LineRenderer _line;

        private List<Vector2Int> _path = new();
        
        private Vector3? _currentTarget;
        private float _mouseDownTime;

        private IEnumerator Start()
        {
            _line = GetComponent<LineRenderer>();
            yield return null;
            MoveToNextPoint();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
                _mouseDownTime = Time.time;
            

            if (Input.GetMouseButtonUp(0) && Time.time - _mouseDownTime < .5f)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(mouseRay, out var info, 100f)) return;

                var tile = info.collider.GetComponent<RenderTile>();
                _path = _pathfinding.FindPath(_hero.Location, tile.Location);

                ApplyPathToLine();
                MoveToNextPoint();
            }

            if (!_currentTarget.HasValue) return;
            
            var diff = _currentTarget.Value - transform.position;
            transform.forward = diff;

            if (diff.magnitude < .1f)
            {
                _hero.Location = _path[0];
                _path.RemoveAt(0);

                MoveToNextPoint();

                if (!_currentTarget.HasValue)
                    _line.positionCount = 0;
                else
                    ApplyPathToLine();

                return;
            }

            transform.position += diff.normalized * Time.deltaTime * 5f;
            if (_line.positionCount > 0)
                _line.SetPosition(0, transform.position);
        }

        private void ApplyPathToLine()
        {
            var pathWorld = new List<Vector3> { transform.position + Vector3.up }
                .Concat(_path.Select(v =>
                    ConfigModel.GetWorldCellPosition(v.x, v.y) + Vector3.up * (_pathfinding.GetY(v) + 2f)))
                .ToArray();
                        
            _line.positionCount = pathWorld.Length;
            _line.SetPositions(pathWorld);
        }

        private void MoveToNextPoint()
        {
            if (_path.Count > 0)
            {
                _currentTarget = ConfigModel.GetWorldCellPosition(_path[0].x, _path[0].y) +
                                 Vector3.up * (_pathfinding.GetY(_path[0]) + 1f);
            }
            else
            {
                _currentTarget = null;
                transform.position = ConfigModel.GetWorldCellPosition(_hero.Location.x, _hero.Location.y) +
                                     Vector3.up * (_pathfinding.GetY(_hero.Location) + 1f);

                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            }
        }
    }
}