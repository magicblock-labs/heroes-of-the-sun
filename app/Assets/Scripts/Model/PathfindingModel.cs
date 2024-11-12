using System.Collections.Generic;
using UnityEngine;
using Utils.Injection;

namespace Model
{
    [Singleton]
    public class PathfindingModel
    {
        private readonly Dictionary<Vector2Int, float> _heightmap = new();

        public void AddPoint(Vector2Int location, float value)
        {
            _heightmap[location] = value;
        }

        readonly Dictionary<Vector2Int, bool> _closedSet = new();
        readonly Dictionary<Vector2Int, bool> _openSet = new();

        //cost of start to this key node
        readonly Dictionary<Vector2Int, int> _gScore = new();

        //cost of start to goal, passing through key node
        readonly Dictionary<Vector2Int, int> _fScore = new();

        readonly Dictionary<Vector2Int, Vector2Int> _nodeLinks = new();

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            _openSet.Clear();
            _closedSet.Clear();
            _gScore.Clear();
            _fScore.Clear();
            _nodeLinks.Clear();

            _openSet[start] = true;
            _gScore[start] = 0;
            _fScore[start] = Heuristic(start, goal);

            while (_openSet.Count > 0)
            {
                var nullableBest = NextBest();
                if (!nullableBest.HasValue)
                    return new List<Vector2Int>();

                var current = nullableBest.Value;
                if (current.Equals(goal))
                {
                    return Reconstruct(current);
                }

                _openSet.Remove(current);
                _closedSet[current] = true;

                foreach (var neighbor in GetNeighbours(current))
                {
                    if (_closedSet.ContainsKey(neighbor))
                        continue;

                    if (!_heightmap.ContainsKey(neighbor))
                        continue;

                    if (GetHeightDiff(current, neighbor) > 1)
                        continue;

                    var projectedG = GetGScore(current) + 1;

                    if (!_openSet.ContainsKey(neighbor))
                        _openSet[neighbor] = true;
                    else if (projectedG >= GetGScore(neighbor))
                        continue;

                    //record it
                    _nodeLinks[neighbor] = current;
                    _gScore[neighbor] = projectedG;
                    _fScore[neighbor] = projectedG + Heuristic(neighbor, goal);
                }
            }


            return new List<Vector2Int>();
        }

        private static int Heuristic(Vector2Int start, Vector2Int goal)
        {
            return Mathf.Abs(goal.x - start.x) + Mathf.Abs(goal.y - start.y);
        }

        private int GetGScore(Vector2Int pt)
        {
            _gScore.TryGetValue(pt, out var score);
            return score;
        }


        private int GetFScore(Vector2Int pt)
        {
            _fScore.TryGetValue(pt, out var score);
            return score;
        }

        private List<Vector2Int> Reconstruct(Vector2Int current)
        {
            var path = new List<Vector2Int>();
            while (_nodeLinks.ContainsKey(current))
            {
                path.Add(current);
                current = _nodeLinks[current];
            }

            path.Reverse();
            return path;
        }

        private Vector2Int? NextBest()
        {
            var best = int.MaxValue;
            Vector2Int? bestPt = null;
            foreach (var node in _openSet.Keys)
            {
                var score = GetFScore(node);
                if (score < best)
                {
                    bestPt = node;
                    best = score;
                }
            }


            return bestPt;
        }


        private static IEnumerable<Vector2Int> GetNeighbours(Vector2Int value)
        {
            return new List<Vector2Int>
            {
                value + Vector2Int.up,
                value + Vector2Int.left,
                value + Vector2Int.down,
                value + Vector2Int.right,
                value + Vector2Int.up + Vector2Int.left,
                value + Vector2Int.up + Vector2Int.right,
                value + Vector2Int.down + Vector2Int.left,
                value + Vector2Int.down + Vector2Int.right,
            };
        }

        private float GetHeightDiff(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(_heightmap.GetValueOrDefault(from, 0) - _heightmap.GetValueOrDefault(to, 0));
        }

        public float GetY(Vector2Int location)
        {
            return _heightmap.GetValueOrDefault(location, 0) ;
        }
    }
}