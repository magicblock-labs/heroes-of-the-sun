using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Newtonsoft.Json;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class RenderPlantResources : InjectableBehaviour, IDisplaySettlementData
    {
        private const float EnvironmentCapacity = 1000f;
        private const float ResourcesPerPlant = 20f;

        [Inject] private ConfigModel _config;
        [Inject] private ResourceLocationModel _locations;

        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject stumpPrefab;

        [SerializeField] private GameObject bushPrefab;
        [SerializeField] private GameObject emptyBushPrefab;

        [SerializeField] private float scaleFactor = 0.95f;


        public void SetData(Settlement.Accounts.Settlement value)
        {
            if (transform.childCount > 0)
                return;

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var i = 0;
            _locations.Reset();
            foreach (var point in GetPlantCoordinates())
            {
                var isWood = i++ * ResourcesPerPlant < EnvironmentCapacity;

                var depleted =
                    isWood
                        ? value.Environment.Wood < i * ResourcesPerPlant
                        : value.Environment.Food > i * ResourcesPerPlant - EnvironmentCapacity;

                if (!depleted)
                    _locations.Add(isWood ? ResourceType.Wood : ResourceType.Food, point);

                var obj = Instantiate(
                    isWood
                        ? !depleted ? treePrefab : stumpPrefab
                        : !depleted
                            ? bushPrefab
                            : emptyBushPrefab,
                    transform,
                    true);

                obj.transform.localPosition = new Vector3(
                    (point.x + .5f) * ConfigModel.CellSize, 0,
                    (point.y + .5f) * ConfigModel.CellSize);
                obj.transform.localScale *= ConfigModel.CellSize * scaleFactor;
            }
        }

        private IEnumerable<Vector2Int> GetPlantCoordinates()
        {
            var cacheString = PlayerPrefs.GetString("ForestLocationCache", null);
            IEnumerable<Vector2Int> randomAndClamped = null;
            if (string.IsNullOrEmpty(cacheString))
            {
                var surroundingCells = new List<Vector2Int>();
                for (var i = -2; i < _config.Width + 2; i++)
                for (var j = -2; j < _config.Height + 2; j++)
                {
                    if (i >= 0 && i < _config.Width && j >= 0 &&
                        j < _config.Height)
                        continue;

                    surroundingCells.Add(new Vector2Int(i, j));
                }

                randomAndClamped = surroundingCells.OrderBy(_ => Guid.NewGuid())
                    .Take((int)Math.Ceiling(2 * EnvironmentCapacity / ResourcesPerPlant))
                    .ToArray(); //2 for food and wood

                PlayerPrefs.SetString("ForestLocationCache", JsonConvert.SerializeObject(randomAndClamped));
            }
            else
            {
                randomAndClamped = JsonConvert.DeserializeObject<Vector2Int[]>(cacheString);
            }

            return randomAndClamped;
        }
    }
}