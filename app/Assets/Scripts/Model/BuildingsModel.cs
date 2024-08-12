using System;
using Service;
using Settlement.Types;
using Utils.Injection;
using Utils.Signal;
using BuildingType = Service.BuildingType;

namespace Model
{
    [Singleton]
    public class BuildingsModel : InjectableObject<BuildingsModel>
    {
        [Inject] private ConfigModel _config;

        public readonly Signal Updated = new();

        public byte[,] OccupiedData { get; private set; }

        private Building[] _data = Array.Empty<Building>();

        public void Set(Building[] value)
        {
            _data = value;
            OccupiedData = GetCellsData();
            Updated.Dispatch();
        }

        public Building[] Get()
        {
            return _data;
        }

        private byte[,] GetCellsData()
        {
            var result = new byte[_config.Width, _config.Height];

            //this is a proper dynamic data calculated based on placed buildings
            foreach (var building in _data)
            {
                var config = _config.Buildings[(BuildingType)building.Id];

                for (var i = building.X; i < building.X + config.width; i++)
                for (var j = building.Y; j < building.Y + config.height; j++)
                    result[i, j] = 1;
            }
            
            return result;
        }
    }
}