using System.Collections.Generic;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class CreateChunks : InjectableBehaviour
    {
        [Inject] private HeroModel _heroModel;

        [SerializeField] private int chunkSize;
        [SerializeField] private int passes = 2;
        [SerializeField] private RenderRandomChunk prefab;

        private readonly Dictionary<Vector2Int, RenderRandomChunk> _visibleChunks = new();

        private void Update()
        {
            var startingChunkX = Mathf.FloorToInt((float)_heroModel.Location.x / chunkSize);
            var startingChunkY = Mathf.FloorToInt((float)_heroModel.Location.y / chunkSize);

            var pass = 0;

            while (pass <= passes)
            {
                for (var chunkX = -pass; chunkX <= pass; chunkX++)
                for (var chunkY = -pass; chunkY <= pass; chunkY++)
                {
                    //edges only
                    if (chunkX > -pass && chunkX < pass && chunkY > -pass && chunkY < pass) continue;

                    var offsetChunkLocation = new Vector2Int(startingChunkX + chunkX, startingChunkY + chunkY);

                    if (!_visibleChunks.ContainsKey(offsetChunkLocation))
                    {
                        _visibleChunks[offsetChunkLocation] =
                            Instantiate(prefab,
                                new Vector3(startingChunkX + chunkX, 0, startingChunkY + chunkY),
                                Quaternion.identity,
                                transform).Create(offsetChunkLocation * chunkSize, chunkSize,
                                new Vector2(chunkSize, chunkSize) * .1f);
                    }
                }

                pass++;
            }
        }
    }
}