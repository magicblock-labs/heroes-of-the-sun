using System;
using System.Diagnostics;
using System.Linq;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    public enum Resource
    {
        Food,
        Wood,
        Water,
        Stone
    }

    public struct StorageCapacity
    {
        public int Food;
        public int Wood;
        public int Water;
        public int Stone;
    }

    [Singleton]
    public class SettlementModel : InjectableObject
    {
        [Inject] private ConfigModel _config;

        public readonly Signal Updated = new();

        public byte[,] OccupiedData { get; private set; }
        public bool HasData => _data != null;

        private Settlement.Accounts.Settlement _data;

        public void Set(Settlement.Accounts.Settlement value)
        {
            _data = value;
            StorageCapacity = GetStorageCapacity();
            OccupiedData = _config.GetCellsData(_data);
            Updated.Dispatch();
        }

        public Settlement.Accounts.Settlement Get()
        {
            return _data;
        }

        public int GetFreeWorkerIndex()
        {
            for (var i = 0; i < _data.WorkerAssignment.Length; i++)
                if (_data.WorkerAssignment[i] == -1)
                    return i;

            return -1;
        }

        public Vector3Int GetUnoccupiedPositionFor(Vector3 dimensions)
        {
            var centerX = _config.Width / 2;
            var centerZ = _config.Height / 2;

            for (var offset = 0; offset < _config.Width / 2; offset++)
            {
                for (var offsetX = -offset; offsetX <= offset; offsetX++)
                for (var offsetZ = -offset; offsetZ <= offset; offsetZ++)
                {
                    if (Math.Abs(offsetX) + Math.Abs(offsetZ) != offset) continue;

                    if (IsValidLocation(centerX + offsetX, centerZ + offsetZ, dimensions))
                        return new Vector3Int(centerX + offsetX, 0, centerZ + offsetZ);
                }
            }

            return new Vector3Int(centerX, 0, centerZ);
        }

        public bool IsValidLocation(int cellPosX, int cellPosZ, Vector3 buildingDimensions)
        {
            for (var i = cellPosX; i < cellPosX + buildingDimensions.x; i++)
            for (var j = cellPosZ; j < cellPosZ + buildingDimensions.z; j++)
            {
                if (i < 0 || i >= OccupiedData.GetLength(0) || j < 0 || j >= OccupiedData.GetLength(1))
                    return false;

                if (OccupiedData[i, j] != 0)
                    return false;
            }


            return true;
        }


        public int GetCollectionLevelMultiplier(byte level)
        {
            return (int)Math.Pow(2, level);
        }


        public StorageCapacity StorageCapacity { get; private set; }

        private static int GetStorageLevelMultiplier(int level)
        {
            return (int)Mathf.Pow(2, level);
        }

        private StorageCapacity GetStorageCapacity()
        {
            var storage = new StorageCapacity()
            {
                Water = 0,
                Food = 0,
                Wood = 0,
            };

            //calc current storage capacity for all resources
            foreach (var building in _data.Buildings)
            {
                if (building.TurnsToBuild > 0)
                    continue;

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (building.Id)
                {
                    case BuildingType.TownHall:
                        storage.Food += building.Level * 10;
                        storage.Water += building.Level * 10;
                        storage.Wood += building.Level * 10;
                        storage.Stone += building.Level * 10;
                        break;

                    case BuildingType.WaterStorage:
                        storage.Water += ConfigModel.WATER_STORAGE_PER_LEVEL *
                                         GetStorageLevelMultiplier(building.Level);
                        break;
                    case BuildingType.FoodStorage:
                        storage.Food += ConfigModel.FOOD_STORAGE_PER_LEVEL * GetStorageLevelMultiplier(building.Level);
                        break;
                    case BuildingType.WoodStorage:
                        storage.Wood += ConfigModel.WOOD_STORAGE_PER_LEVEL * GetStorageLevelMultiplier(building.Level);
                        break;
                    case BuildingType.StoneStorage:
                        storage.Stone += ConfigModel.STONE_STORAGE_PER_LEVEL *
                                         GetStorageLevelMultiplier(building.Level);
                        break;
                }
            }

            var storageResearch = GetResearchLevel(ResearchType.StorageCapacity);
            var storageMultiplier = 1.0f + ConfigModel.STORAGE_CAPACITY_RESEARCH_MULTIPLIER * storageResearch;

            if (!(storageMultiplier > 1.0)) return storage;

            storage.Water = (int)Math.Floor(storage.Water * storageMultiplier);
            storage.Food = (int)Math.Floor(storage.Food * storageMultiplier);
            storage.Wood = (int)Math.Floor(storage.Wood * storageMultiplier);
            storage.Stone = (int)Math.Floor(storage.Stone * storageMultiplier);

            return storage;
        }

        public enum ResearchType
        {
            BuildingSpeed,
            BuildingCost,
            DeteriorationCap,
            Placeholder,
            StorageCapacity,
            ResourceCollectionSpeed,
            EnvironmentRegeneration,
            Mining,
            ExtraUnit,
            DeathTimeout,
            Consumption,
            Placeholder2,
            MaxEnergyCap,
            EnergyRegeneration,
            FaithBonus,
            Placeholder3,
        }

        private const int BitsPerResearch = 2;
        private const int ResearchMask = 3; //00000011

        public uint GetResearchLevel(ResearchType researchType)
        {
            var shiftBy = BitsPerResearch * (int)researchType;
            return (_data.Research >> shiftBy) & ResearchMask;
        }

        public int GetConsumptionRate()
        {
            var result = _data.WorkerAssignment.Count(buildingId => buildingId >= -1);

            result = Math.Max(0,
                result - (int)GetResearchLevel(ResearchType.Consumption));

            return result;
        }

        public int GetCollectionRate(BuildingType type)
        {
            var result = 0;

            if (type == BuildingType.WaterCollector) //wells dont need workers
            {
                result += _data.Buildings.Where(building => building.Id == type)
                    .Sum(GetBuildingCollectionRate);
            }
            else
            {
                result += (from buildingId in _data.WorkerAssignment
                    where buildingId >= 0
                    select _data.Buildings[buildingId]
                    into building
                    where building.Id == type
                    select GetBuildingCollectionRate(building)).Sum();
            }

            return result;
        }

        private int GetBuildingCollectionRate(Building building)
        {
            if (building.TurnsToBuild > 0 || building.Deterioration >= 127)
                return 0;

            return GetCollectionLevelMultiplier(building.Level) +
                   (int)GetResearchLevel(ResearchType.ResourceCollectionSpeed);
        }

        private const int BaseEnergyCap = 30;
        private const float EnergyCapFaithMultiplier = 0.1f;
        private const int MaxEnergyCapResearchMultiplier = 1;

        public int GetEnergyCap()
        {
            return BaseEnergyCap
                   + (int)(_data.Faith * EnergyCapFaithMultiplier)
                   + MaxEnergyCapResearchMultiplier
                   * (int)GetResearchLevel(ResearchType.MaxEnergyCap);
        }

        private const int BaseMinutePerEnergyUnit = 10;
        private const int EnergyRegenResearchMultiplier = 1;
        private const int SecondsInMinute = 60;
        private const float EnergyRegenFaithMultiplier = 0.05f;
        private const float BuildingCostResearchMultiplier = 0.1f;
        private const int BuildingSpeedResearchTurnReduction = 1;


        public static ResourceBalance GetExchangeRates()
        {
            return new ResourceBalance()
            {
                Water = 10,
                Food = 10,
                Wood = 6,
                Stone = 3,
            };
        }

        public long GetNextEnergyClaimTimestamp()
        {
            var secondsPerUnit = SecondsInMinute
                                 * (BaseMinutePerEnergyUnit
                                    - (int)(EnergyRegenResearchMultiplier
                                            * GetResearchLevel(ResearchType.EnergyRegeneration))
                                    - (int)(_data.Faith * EnergyRegenFaithMultiplier));

            return _data.LastTimeClaim + secondsPerUnit;
        }


        private ushort CalculateCost(uint tier, int level, int levelOffset, float multiplier)
        {
            if (levelOffset >= level)
            {
                return 0;
            }

            const int baseCost = 2;
            const float levelMultiplier = 1.5f;
            var costResearch = GetResearchLevel(ResearchType.BuildingCost);
            var cost = baseCost * tier * Math.Pow(levelMultiplier, level - levelOffset)
                       * (1.0 - BuildingCostResearchMultiplier * costResearch)
                       * multiplier;

            return (ushort)Math.Ceiling(cost);
        }

        public ResourceBalance GetConstructionCost(uint tier, int level, float multiplier)
        {
            return new ResourceBalance
            {
                Food = 0,
                Water = 0,
                Wood = CalculateCost(tier, level, 0, multiplier),
                Stone = CalculateCost(tier, level, 4, multiplier)
            };
        }

        public int GetBuildTime(uint tier, int level)
        {
            const float levelMultiplier = 1.2f;
            var baseCost = (int)(tier * Math.Pow(levelMultiplier, level));
            return baseCost
                   - (int)Math.Min(
                       BuildingSpeedResearchTurnReduction * GetResearchLevel(ResearchType.BuildingSpeed),
                       baseCost
                   );
        }

        public float GetMaxDeterioration()
        {
            return ConfigModel.BASE_DETERIORATION_CAP
                   + ConfigModel.DETERIORATION_CAP_RESEARCH_MULTIPLIER
                   * GetResearchLevel(ResearchType.DeteriorationCap);
        }

        private const int BaseResearchCost = 5;

        public int GetResearchCost(ResearchType type)
        {
            return BaseResearchCost * (int)Math.Pow(2, GetResearchLevel(type));
        }

        public uint GetQuestProgress(QuestData data)
        {
            return data.type switch
            {
                QuestType.Build => (uint)_data.Buildings.Count(b => b.Id == (BuildingType)data.targetType),
                QuestType.Upgrade => _data.Buildings.Where(b => b.Id == (BuildingType)data.targetType)
                    .Max(b => b.Level),
                QuestType.Store => (Resource)data.targetType switch
                {
                    Resource.Food => _data.Treasury.Food,
                    Resource.Wood => _data.Treasury.Wood,
                    Resource.Water => _data.Treasury.Water,
                    Resource.Stone => _data.Treasury.Stone,
                    _ => throw new ArgumentOutOfRangeException()
                },
                QuestType.Research => GetResearchLevel((ResearchType)data.targetType),
                QuestType.Faith => _data.Faith,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}