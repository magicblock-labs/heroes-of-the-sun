using Service;
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
    }
}