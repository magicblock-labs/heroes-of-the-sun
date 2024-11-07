using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Settlement;
using Settlement.Program;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;

namespace Connectors
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class OtherPlayersSettlementConnector : BaseProgramConnector<SettlementClient>
    {
        public Vector2Int? Location;
        public string PreferenceKey => $"{nameof(Settlement)}@{GetExtraSeed()}";

        protected override string GetExtraSeed()
        {
            return $"{Location?.x}x{Location?.y}";
        }

        public override PublicKey GetComponentProgramAddress()
        {
            return new("B2h45ZJwpiuD9jBY7Dfjky7AmEzdzGsty4qWQxjX9ycv");
        }

        private SettlementClient Settlement =>
            Client ??= new SettlementClient(Web3.Rpc, Web3.WsRpc, new PublicKey(SettlementProgram.ID));

        public async Task<Settlement.Accounts.Settlement> LoadData()
        {
            if (!Location.HasValue)
                throw new Exception("Settlement connector needs a valid location");

            var settlementJson = PlayerPrefs.GetString(PreferenceKey, null);
            
            //todo add a converter to omit bolt metadata
            // if (!string.IsNullOrEmpty(settlementJson))
            //     return JsonConvert.DeserializeObject<Settlement.Accounts.Settlement>(settlementJson);
            //
            var rawData = await Settlement.GetSettlementAsync(
                await GetComponentDataAddress(), 
                Commitment.Processed);
            if (rawData.ParsedResult == null) return null;

            settlementJson = JsonConvert.SerializeObject(rawData.ParsedResult);
            Debug.Log($"Data:\n {settlementJson}");
            PlayerPrefs.SetString(PreferenceKey, settlementJson);

            return rawData.ParsedResult;
        }
    }
}