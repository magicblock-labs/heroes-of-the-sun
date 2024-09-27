using System;
using Service;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
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
            var workerAllocation = _data.LabourAllocation;
            for (var i = 0; i < workerAllocation.Length; i++)
            {
                if (workerAllocation[i] == -1)
                {
                    return i;
                }
            }

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
    }
}