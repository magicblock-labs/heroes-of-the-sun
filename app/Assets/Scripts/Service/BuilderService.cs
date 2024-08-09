using System;
using System.Threading.Tasks;
using Data;
using Model;
using Newtonsoft.Json;
using UnityEngine;
using Utils.Injection;

namespace Service
{
    [Singleton]
    public class BuilderService : InjectableObject<BuilderService>
    {
        [Inject] private RemoteConnectorMock _connectorMock;

        [Inject] private BalanceModel _balance;
        [Inject] private BuildingsModel _buildings;
        [Inject] private AgentsModel _agents;
        [Inject] private ConfigModel _config;

        public async Task<bool> ReloadData()
        {
            Debug.Log("ReloadData");

            var gameDataRaw = await _connectorMock.GetGameState();

            if (string.IsNullOrEmpty(gameDataRaw))
                return false;

            try
            {
                var gameData = JsonConvert.DeserializeObject<GameState>(gameDataRaw);

                _balance.Set(gameData.Credits);
                _buildings.Set(gameData.Buildings);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return false;
        }

        public async Task<bool> Initialise()
        {
            return await _connectorMock.InitGameState();
        }

        public async Task<bool> PlaceBuilding(byte x, byte y, byte id)
        {
            var stateError = await _connectorMock.PlaceBuilding(x, y, id);
            var ok = stateError == StateError.None;
            
            if (!ok)
                Debug.LogError(stateError);
            
            return ok;
        }
    }
}