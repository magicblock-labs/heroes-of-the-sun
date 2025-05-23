using System;
using Connectors;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderGenericChunk : InjectableBehaviour
    {
        [Inject] private SettlementConnector _settlement;

        [SerializeField] private RenderRandomChunk randomPrefab;
        [SerializeField] private RenderSettlementChunk settlementPrefab;

        [SerializeField] private int chunksPerSettlement;

        public async void Create(Vector2Int location, int chunkSize, bool instant)
        {
            transform.position = new Vector3(location.x * ConfigModel.CellSize, 0, location.y * ConfigModel.CellSize) *
                                 chunkSize;

            //try to load settlement at location.
            if (location.x % chunksPerSettlement == 0 && location.y % chunksPerSettlement == 0)
            {
                var settlementLocation = location / chunksPerSettlement;
                await _settlement.SetSeed($"{settlementLocation.x}_{settlementLocation.y}", false);
                var data = await _settlement.LoadData();

                //could have been destroyed during the await thing
                try
                {
                    gameObject.name = $"Settlement@{location.x}x{location.y}";
                    Instantiate(settlementPrefab, transform).Create(data);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message); //can throw null as you can leave the scene before await is done
                }
            }
            else
            {
                //if none found, render a perlin chunk
                gameObject.name = $"RandomChunk@{location.x}x{location.y}";
                Instantiate(randomPrefab, transform).Create(location * chunkSize, chunkSize,
                    new Vector2(chunkSize, chunkSize) * .1f, instant);
            }
        }
    }
}