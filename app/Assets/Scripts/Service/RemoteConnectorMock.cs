using System;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using Utils.Injection;
using View;

namespace Service
{
    [Singleton]
    public class RemoteConnectorMock : InjectableObject<RemoteConnectorMock>
    {
        [Inject] private ConfigModel _config;

        private string _accountId;
        private const string DefaultState = "{\"Buildings\":[], \"Credits\":1000}";

        public string AccountId => _accountId;

        public void SignIn()
        {
            if (_accountId == null)
                if (PlayerPrefs.HasKey(nameof(AccountId)))
                    _accountId = PlayerPrefs.GetString(nameof(AccountId), null);

            if (_accountId != null) return;

            _accountId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(nameof(AccountId), _accountId);
        }

        public async Task<string> GetGameState()
        {
            return PlayerPrefs.GetString(nameof(GameState), null);
        }

        public async Task<bool> InitGameState()
        {
            PlayerPrefs.SetString(nameof(GameState), DefaultState);
            return true;
        }

        public async Task ResetAccounts()
        {
            PlayerPrefs.DeleteAll();
        }

        public async Task<StateError> PlaceBuilding(byte x, byte y, byte id)
        {
            var raw = await GetGameState();

            //obviously this serialisation back and forth is a mock, emulating the contract state
            var parsed = JsonConvert.DeserializeObject<GameState>(raw);

            var config = _config.Buildings[(BuildingType)id];
            if (config.cost > parsed.Credits)
                return StateError.NotEnoughCredits;

            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            parsed.Buildings = parsed.Buildings
                .Append(new Building { X = x, Y = y, Id = id, Level = 1, Timestamp = nowUnix }).ToArray();

            parsed.Credits -= config.cost;
            
            PlayerPrefs.SetString(nameof(GameState), JsonConvert.SerializeObject(parsed));

            return StateError.None;
        }
    }
}