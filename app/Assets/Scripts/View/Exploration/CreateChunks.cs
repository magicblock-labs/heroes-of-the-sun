using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class CreateChunks : InjectableBehaviour
    {
        [Inject] private PlayerHeroModel _playerHero;
        
        [SerializeField] private int chunkSize;
        [SerializeField] private int passes = 2;
        [SerializeField] private RenderGenericChunk prefab;

        private readonly HashSet<Vector2Int> _initialisedChunks = new();

        private void Update()
        {
            if (!_playerHero.HasData)
                return;
            
            var startingChunkX = Mathf.FloorToInt((float)_playerHero.Get().X / chunkSize);
            var startingChunkY = Mathf.FloorToInt((float)_playerHero.Get().Y / chunkSize);

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