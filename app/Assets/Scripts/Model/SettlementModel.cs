using System;
using Service;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    public struct StorageCapacity
    {
        public int Food;
        public int Wood;
        public int Water;
    }

    [Singleton]
    public class SettlementModel : InjectableObject<SettlementModel>
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
            OccupiedData = GetCellsData();
            Updated.Dispatch();
        }

        public Settlement.Accounts.Settlement Get()
        {
            return _data;
        }

        private byte[,] GetCellsData()
        {
            var result = new byte[_config.Width, _config.Height];

            //this is a proper dynamic data calculated based on placed buildings
            foreach (var building in _data.Buildings)
            {
                var config = _config.Buildings[(BuildingType)building.Id];

                for (var i = building.X; i < building.X + config.width; i++)
                for (var j = building.Y; j < building.Y + config.height; j++)
                    result[i, j] = 1;
            }

            return result;
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


        public StorageCapacity StorageCapacity { get; private set; }

        private static int GetLevelMultiplier(int level)
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
                    case Settlement.Types.BuildingType.WaterStorage:
                        storage.Water += ConfigModel.WATER_STORAGE_PER_LEVEL * GetLevelMultiplier(building.Level);
                        break;
                    case Settlement.Types.BuildingType.FoodStorage:
                        storage.Food += ConfigModel.FOOD_STORAGE_PER_LEVEL * GetLevelMultiplier(building.Level);
                        break;
                    case Settlement.Types.BuildingType.WoodStorage:
                        storage.Wood += ConfigModel.WOOD_STORAGE_PER_LEVEL * GetLevelMultiplier(building.Level);
                        break;
                }
            }

            var storageResearch = GetResearchLevel(ResearchType.StorageCapacity);
            var storageMultiplier = 1.0f + ConfigModel.STORAGE_CAPACITY_RESEARCH_MULTIPLIER * storageResearch;
            
            if (!(storageMultiplier > 1.0)) return storage;
            
            storage.Water = (int)Math.Floor(storage.Water * storageMultiplier);
            storage.Food = (int)Math.Floor(storage.Food * storageMultiplier);
            storage.Wood = (int)Math.Floor(storage.Wood * storageMultiplier);

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
    }
}