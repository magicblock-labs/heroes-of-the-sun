using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderTile : InjectableBehaviour
    {
        [Inject] private PathfindingModel _pathfinder;

        [SerializeField] private GameObject[] tiles;
        [SerializeField] private GameObject tree;
        public Vector2Int Location { get; set; }

        public RenderTile Create(Vector2Int offset, Vector2Int position, float value, bool edge)
        {
            gameObject.name = $"Tile@{transform.localPosition.x}x{transform.localPosition.z}";
            var yPos = (int)(value * tiles.Length * 4) - 6.5f;

            Location = offset + position;
            _pathfinder.AddPoint(Location, yPos);

            var tileIndex = Mathf.Min((int)(tiles.Length * value), tiles.Length - 1);
            var prefab = tiles[tileIndex];
            Instantiate(prefab, transform);

            transform.localScale = Vector3.one * 2;
            transform.localPosition = ConfigModel.GetWorldCellPosition(position.x, position.y) +
                                      Vector3.up * yPos;

            if (tree != null && tileIndex == tiles.Length - 1 && position.x % 2 == 0 && position.y % 2 == 0)
                Instantiate(tree, transform);

            if (edge)
            {
                for (var i = 1; i < yPos; i++)
                {
                    Instantiate(prefab, transform).transform.localPosition = Vector3.down * i;
                }
            }

            //setup collider
            foreach (var c in GetComponentsInChildren<Collider>())
                Destroy(c);

            var coll = gameObject.AddComponent<BoxCollider>();
            coll.size = Vector3.one;

            return this;
        }
    }
}