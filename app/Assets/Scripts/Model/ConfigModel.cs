using System.Collections.Generic;
using Utils.Injection;

namespace Service
{
    public enum BuildingType
    {
        None = -1,
        TownHall = 0,
        WaterCollector = 1,
        FoodCollector = 2,
        WoodCollector = 3,
        WaterStorage = 4,
        FoodStorage = 5,
        WoodStorage = 6,
        Altar = 7,
    }

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
        private readonly BuildConfig _buildConfig = new()
        {
            width = 48,
            height = 48,
            buildings = new Dictionary<BuildingType, BuildingConfig>
            {
                {
                    BuildingType.TownHall, new BuildingConfig
                    {
                        width = 4,
                        height = 4,
                        cost = 30,
                        buildTime = 6,
                        prefab = BuildingType.TownHall.ToString()
                    }
                },
                {
                    BuildingType.WaterCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 30,
                        buildTime = 6,
                        prefab = BuildingType.WaterCollector.ToString()
                    }
                },
                {
                    BuildingType.FoodCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 30,
                        buildTime = 10,
                        prefab = BuildingType.FoodCollector.ToString()
                    }
                },
                {
                    BuildingType.WoodCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        cost = 30,
                        buildTime = 10,
                        prefab = BuildingType.WoodCollector.ToString()
                    }
                },
                {
                    BuildingType.WaterStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 30,
                        buildTime = 10,
                        prefab = BuildingType.WaterStorage.ToString()
                    }
                },
                {
                    BuildingType.FoodStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 30,
                        buildTime = 10,
                        prefab = BuildingType.FoodStorage.ToString()
                    }
                },
                {
                    BuildingType.WoodStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        cost = 30,
                        buildTime = 10,
                        prefab = BuildingType.WoodStorage.ToString()
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
                }
            }
        };

        public int Width => _buildConfig.width;
        public int Height => _buildConfig.height;
        public Dictionary<BuildingType, BuildingConfig> Buildings => _buildConfig.buildings;
    }
}