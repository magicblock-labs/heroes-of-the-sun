using System.Collections;
using System.Collections.Generic;
using Settlement.Types;
using UnityEditor;
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
        public uint costTier;
        public uint buildTimeTier;
        public string prefab;
    }


    [Singleton]
    public class ConfigModel
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
                        costTier = 5,
                        buildTimeTier = 5,
                        prefab = BuildingType.TownHall.ToString()
                    }
                },
                {
                    BuildingType.Altar, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        costTier = 3,
                        buildTimeTier = 5,
                        prefab = BuildingType.Altar.ToString()
                    }
                },
                {
                    BuildingType.Research, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        costTier = 3,
                        buildTimeTier = 5,
                        prefab = BuildingType.Research.ToString()
                    }
                },
                {
                    BuildingType.FoodCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        costTier = 1,
                        buildTimeTier = 2,
                        prefab = BuildingType.FoodCollector.ToString()
                    }
                },
                {
                    BuildingType.FoodStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        costTier = 2,
                        buildTimeTier = 4,
                        prefab = BuildingType.FoodStorage.ToString()
                    }
                },
                {
                    BuildingType.WoodCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        costTier = 1,
                        buildTimeTier = 2,
                        prefab = BuildingType.WoodCollector.ToString()
                    }
                },
                {
                    BuildingType.WoodStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        costTier = 2,
                        buildTimeTier = 4,
                        prefab = BuildingType.WoodStorage.ToString()
                    }
                },
                {
                    BuildingType.WaterCollector, new BuildingConfig
                    {
                        width = 2,
                        height = 2,
                        costTier = 1,
                        buildTimeTier = 2,
                        prefab = BuildingType.WaterCollector.ToString()
                    }
                },
                {
                    BuildingType.WaterStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        costTier = 2,
                        buildTimeTier = 4,
                        prefab = BuildingType.WaterStorage.ToString()
                    }
                },
                {
                    BuildingType.StoneCollector, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        costTier = 1,
                        buildTimeTier = 2,
                        prefab = BuildingType.StoneCollector.ToString()
                    }
                },
                {
                    BuildingType.StoneStorage, new BuildingConfig
                    {
                        width = 3,
                        height = 3,
                        costTier = 2,
                        buildTimeTier = 4,
                        prefab = BuildingType.StoneStorage.ToString()
                    }
                }
            }
        };

        public const int BASE_DETERIORATION_CAP = 50;
        public const int DETERIORATION_CAP_RESEARCH_MULTIPLIER = 5;

        public int Width => _buildConfig.width;
        public int Height => _buildConfig.height;
        public Dictionary<BuildingType, BuildingConfig> Buildings => _buildConfig.buildings;

        public static Vector3 GetWorldCellPosition(int x, int y)
        {
            return new Vector3(
                (x + 0.5f) * CellSize, 0,
                (y + 0.5f) * CellSize);
        }

        public byte[,] GetCellsData(Settlement.Accounts.Settlement data)
        {
            var result = new byte[Width, Height];

            if (data != null)
                foreach (var building in data.Buildings)
                {
                    var config = Buildings[building.Id];

                    for (var i = building.X; i < building.X + config.width; i++)
                    for (var j = building.Y; j < building.Y + config.height; j++)
                        result[i, j] = 1;
                }

            return result;
        }

        public static QuestData[] Quests =
        {
            new()
            {
                type = QuestType.Build,
                targetType = (int)BuildingType.FoodStorage,
                rewardType = (int)Resource.Wood,
                rewardValue = 10
            },
            new()
            {
                type = QuestType.Build,
                targetType = (int)BuildingType.FoodCollector,
                rewardType = (int)Resource.Water,
                rewardValue = 10
            },
            new()
            {
                type = QuestType.Build,
                targetType = (int)BuildingType.WaterStorage,
                rewardType = (int)Resource.Water,
                rewardValue = 10
            },
            new()
            {
                type = QuestType.Build,
                targetType = (int)BuildingType.WaterCollector,
                rewardType = (int)Resource.Water,
                rewardValue = 10
            },

            new()
            {
                type = QuestType.Upgrade,
                targetType = (int)BuildingType.TownHall,
                targetValue = 2,
                rewardType = (int)Resource.Wood,
                rewardValue = 100
            },

            new()
            {
                type = QuestType.Upgrade,
                targetType = (int)BuildingType.WoodCollector,
                targetValue = 2,
                rewardType = (int)Resource.Water,
                rewardValue = 20
            },

            new()
            {
                type = QuestType.Upgrade,
                targetType = (int)BuildingType.TownHall,
                targetValue = 3,
                rewardType = (int)Resource.Stone,
                rewardValue = 10
            },

            new()
            {
                type = QuestType.Store,
                targetType = (int)Resource.Food,
                targetValue = 30,
                rewardType = (int)Resource.Stone,
                rewardValue = 5
            },
            new()
            {
                type = QuestType.Store,
                targetType = (int)Resource.Wood,
                targetValue = 50,
                rewardType = (int)Resource.Stone,
                rewardValue = 5
            },
            new()
            {
                type = QuestType.Store,
                targetType = (int)Resource.Stone,
                targetValue = 30,
                rewardType = (int)Resource.Stone,
                rewardValue = 20
            },
            new()
            {
                type = QuestType.Research,
                targetType = (int)SettlementModel.ResearchType.BuildingCost,
                targetValue = 1,
                rewardType = (int)Resource.Stone,
                rewardValue = 5
            },
            new()
            {
                type = QuestType.Research,
                targetType = (int)SettlementModel.ResearchType.Consumption,
                targetValue = 1,
                rewardType = (int)Resource.Stone,
                rewardValue = 5
            },
            new()
            {
                type = QuestType.Faith,
                targetValue = 30,
                rewardType = (int)Resource.Stone,
                rewardValue = 15
            },
            new()
            {
                type = QuestType.Faith,
                targetValue = 60,
                rewardType = (int)Resource.Stone,
                rewardValue = 30
            },
        };

        public Dictionary<QuestType, QuestData> GetFirstUnclaimedQuests(ulong progress)
        {
            var result = new Dictionary<QuestType, QuestData>();
            foreach (var quest in Quests)
            {
                //if claimd continue

                result.TryAdd(quest.type, quest);
            }
                
            return result;
        }
    }
}