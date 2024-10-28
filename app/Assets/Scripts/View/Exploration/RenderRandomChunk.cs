using Model;
using Unity.AI.Navigation;
using UnityEngine;

public class RenderRandomChunk : MonoBehaviour
{
    [SerializeField] private RenderTile tile;

    public RenderRandomChunk Create(Vector2Int offset, int size, Vector2 scale)
    {
        transform.position = new Vector3(offset.x * ConfigModel.CellSize, 0, offset.y * ConfigModel.CellSize);

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        gameObject.name = $"Chunk@{transform.position.x}x{transform.position.z}";

        for (var x = 0; x < size; x++)
        for (var y = 0; y < size; y++)
        {
            var sampleX = (float)(x + offset.x) / size * scale.x;
            var sampleY = (float)(y + offset.y) / size * scale.y;

            var perlinNoiseSample = Mathf.PerlinNoise(sampleX, sampleY);
            Instantiate(tile, transform).Create(offset, new Vector2Int(x, y), perlinNoiseSample);
        }

        return this;
    }
}