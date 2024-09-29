using System.Collections.Generic;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;

namespace Model
{
    public class BuildConfig
    {
        public int width;
        public int height;
        public Dictionary<BuildingType, BuildingConfig> buildings;
    }

    public class BuildingConfig
    {
        public int width;
        public int height;
        public uint cost;
        public uint buildTime;
        public string prefab;
    }


    [Singleton]
    public class ConfigModel : InjectableObject<ConfigModel>
    {
        public const int CellSize = 2; //units per cell


        public const int WATER_STORAGE_PER_LEVEL = 10;
        public const int FOOD_STORAGE_PER_LEVEL = 20;
        public const int WOOD_STORAGE_PER_LEVEL = 50;
        public const int STONE_STORAGE_PER_LEVEL = 15;
        public const int GOLD_STORAGE_PER_LEVEL = 5;


        public const float STORAGE_CAPACITY_RESEARCH_MULTIPLIER = 0.1f;

        private readonly BuildConfig _buildConfig = new()
        {
            width = 20,
            height = 20,
            buildings = new Dictionary<BuildingType, BuildingConfig>
            {
                {
                    BuildingType.TownHall, new BuildingConfig
                    {
                        width = 4,
                        height = 4,
                        cost = 50,
                        buildTime = 3,
                        prefab = BuildingType.TownHall.ToString()
                    }
                },
                {
                    BuildingType.Altar, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 30,
                        buildTime = 10,
                        prefab = BuildingType.Altar.ToString()
                    }
                },
                {
                    BuildingType.Research, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 60,
                        buildTime = 5,
                        prefab = BuildingType.Research.ToString()
                    }
                },
                {
                    BuildingType.WaterCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 10,
                        buildTime = 2,
                        prefab = BuildingType.WaterCollector.ToString()
                    }
                },
                {
                    BuildingType.WaterStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 5,
                        buildTime = 2,
                        prefab = BuildingType.WaterStorage.ToString()
                    }
                },
                {
                    BuildingType.FoodCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 10,
                        buildTime = 1,
                        prefab = BuildingType.FoodCollector.ToString()
                    }
                },
                {
                    BuildingType.FoodStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 8,
                        buildTime = 2,
                        prefab = BuildingType.FoodStorage.ToString()
                    }
                },
                {
                    BuildingType.WoodCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 5,
                        buildTime = 3,
                        prefab = BuildingType.WoodCollector.ToString()
                    }
                },
                {
                    BuildingType.WoodStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 8,
                        buildTime = 4,
                        prefab = BuildingType.WoodStorage.ToString()
                    }
                },
                {
                    BuildingType.StoneCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 5,
                        buildTime = 3,
                        prefab = BuildingType.StoneCollector.ToString()
                    }
                },
                {
                    BuildingType.StoneStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 8,
                        buildTime = 4,
                        prefab = BuildingType.StoneStorage.ToString()
                    }
                },
                {
                    BuildingType.GoldCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 5,
                        buildTime = 3,
                        prefab = BuildingType.GoldCollector.ToString()
                    }
                },
                {
                    BuildingType.GoldStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 8,
                        buildTime = 4,
                        prefab = BuildingType.GoldStorage.ToString()
                    }
                }
            }
        };

        public int Width => _buildConfig.width;
        public int Height => _buildConfig.height;
        public Dictionary<BuildingType, BuildingConfig> Buildings => _buildConfig.buildings;

        public static Vector3 GetWorldCellPosition(int i, int j)
        {
            return new Vector3(
                (i + 0.5f) * CellSize, 0,
                (j + 0.5f) * CellSize);
        }
    }
}