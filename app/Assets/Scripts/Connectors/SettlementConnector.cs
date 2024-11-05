using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Settlement;
using Settlement.Program;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;

namespace Connectors
{
    [Singleton]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SettlementConnector : BaseProgramConnector<SettlementClient>
    {
        [Inject] private SettlementModel _settlement;

        public Vector2Int? Location;

        protected override string GetExtraSeed()
        {
            return $"{Location?.x}x{Location?.y}";
        }

        protected override PublicKey GetComponentProgramAddress()
        {
            return new("B2h45ZJwpiuD9jBY7Dfjky7AmEzdzGsty4qWQxjX9ycv");
        }

        private SettlementClient Settlement =>
            Client ??= new SettlementClient(Web3.Rpc, Web3.WsRpc, new PublicKey(SettlementProgram.ID));
        
        public async Task ReloadData()
        {
            if (!Location.HasValue)
                throw new Exception("Settlement connector needs a valid location"); 
                    
            var rawData = await Settlement.GetSettlementAsync(
                await GetComponentDataAddress(), 
                Commitment.Processed);
            if (rawData.ParsedResult == null) return;

            Debug.Log($"Data:\n {JsonConvert.SerializeObject(rawData.ParsedResult)}");

            _settlement.Set(rawData.ParsedResult);
        }

        public async Task<bool> PlaceBuilding(byte x, byte y, byte type, int worker_index)
        {
            return await ApplySystem(new PublicKey("AoKVKur4mczZtuzeMQwydkMe6ZSrJGxTWqZU6grPnd9c"),
                new { x, y, config_index = type, worker_index });
        }

        public async Task<bool> Wait(int time)
        {
            return await ApplySystem(new PublicKey("5LiZ8jP6fqAWT5V6B3C13H9VCwiQoqdyPwUYzWDfMUSy"),
                new { time });
        }

        public async Task<bool> AssignLabour(int worker_index, int building_index)
        {
            return await ApplySystem(new PublicKey("F7m12a5YbScFwNPrKXwg4ua6Z9e7R1ZqXvXigoUfFDMq"),
                new { worker_index, building_index });
        }


        public async Task<bool> Repair(int index)
        {
            return await ApplySystem(new PublicKey("4MA6KhwEUsLbZJqJK9rqwVjdZgdxy7vbebuD2MeLKm5j"), new { index });
        }

        public async Task<bool> Upgrade(int index, int worker_index)
        {
            return await ApplySystem(new PublicKey("J3evfUppPdgjTzWhhAhuhKBVM23UU8iCU9j9r7sTHCTB"),
                new { index, worker_index });
        }

        public async Task<bool> ClaimTime()
        {
            return await ApplySystem(new PublicKey("HFx2weMbr8CrAEAPfPtgw9zzgHgUFzSz7qiTyhTHGSF"), new { });
        }

        public async Task<bool> Research(int research_type)
        {
            return await ApplySystem(new PublicKey("GnVJxqk8dExpXhVidSEFNQcjTY1sCAYWcwM1GGVKKVHb"),
                new { research_type });
        }

        public async Task<bool> Sacrifice(int index)
        {
            return await ApplySystem(new PublicKey("4Cvjz6qrVakbSg3dqBMA8vv8XL8KD3UCTbRVM8g8WkoW"),
                new { index });
        }

        public async Task<bool> Reset()
        {
            return await ApplySystem(new PublicKey("J2HTjpKDf317Q7Pg9kUVFDregE2Ld34P61M5m4XnVSh2"),
                new { });
        }
    }
}