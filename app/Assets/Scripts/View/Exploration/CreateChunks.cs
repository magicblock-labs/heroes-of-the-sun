using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class CreateChunks : InjectableBehaviour
    {
        [SerializeField] private int chunkSize;
        [SerializeField] private int passes = 2;
        [SerializeField] private RenderGenericChunk prefab;

        private readonly HashSet<Vector2Int> _initialisedChunks = new();

        private void Update()
        {
            var startingChunkX = 0;// Mathf.FloorToInt((float)_playerHeroLocationModel.Location.x / chunkSize);
            var startingChunkY = 0;// Mathf.FloorToInt((float)_playerHeroLocationModel.Location.y / chunkSize);

            var pass = 0;

            while (pass <= passes)
            {
                for (var chunkX = -pass; chunkX <= pass; chunkX++)
                for (var chunkY = -pass; chunkY <= pass; chunkY++)
                {
                    //edges only
                    if (chunkX > -pass && chunkX < pass && chunkY > -pass && chunkY < pass) continue;

                    var offsetChunkLocation = new Vector2Int(startingChunkX + chunkX, startingChunkY + chunkY);

                    if (_initialisedChunks.Add(offsetChunkLocation))
                        Instantiate(prefab, transform).Create(offsetChunkLocation, chunkSize, pass < 2);
                }

                pass++;
            }
        }
    }
}