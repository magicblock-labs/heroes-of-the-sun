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

        public RenderTile Create(Vector2Int offset, Vector2Int position,  float value)
        {
            var tileIndex = Mathf.Min((int)(tiles.Length * value), tiles.Length - 1);
            var prefab = tiles[tileIndex];
            Instantiate(prefab, transform);

            transform.localScale = Vector3.one * 2;
            var yPos = (int)(value * tiles.Length * 4);
            transform.localPosition = ConfigModel.GetWorldCellPosition(position.x, position.y) +
                                      Vector3.up * yPos;

            if (tileIndex == tiles.Length-1 && position.x % 2 == 0 && position.y % 2 == 0)
                Instantiate(tree, transform);
        
            gameObject.name = $"Tile@{transform.localPosition.x}x{transform.localPosition.z}";

            foreach (var c in GetComponentsInChildren<Collider>())
                Destroy(c);
        
            var coll = gameObject.AddComponent<BoxCollider>();
            coll.size = Vector3.one;

            Location = offset + position;
            _pathfinder.AddPoint(Location, yPos);
        

            return this;
        }
    }
}