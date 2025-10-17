using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderTile : MonoBehaviour
    {
        public Vector2Int Location { get; private set; }

        public RenderTile Create(Vector2Int offset, Vector2Int position)
        {
            gameObject.name = $"Tile@{transform.localPosition.x}x{transform.localPosition.z}";
            Location = offset + position;

            //setup collider
            foreach (var c in GetComponentsInChildren<Collider>())
                Destroy(c);

            var coll = gameObject.AddComponent<BoxCollider>();
            coll.size = Vector3.one;

            return this;
        }
    }
}