using System.Threading.Tasks;
using Locationallocator;
using Locationallocator.Accounts;
using Locationallocator.Program;
using Newtonsoft.Json;
using Player;
using Player.Program;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;

// ReSharper disable InconsistentNaming (this is for args passing without renaming etc to match rust)

namespace Connectors
{
    [Singleton]
    public class LocationAllocatorConnector : BaseProgramConnector<LocationallocatorClient>
    {
        private LocationallocatorClient LocationAllocator =>
            _client ??= new LocationallocatorClient(Web3.Rpc, Web3.WsRpc, new PublicKey(LocationallocatorProgram.ID));

        protected override string GetExtraSeed()
        {
            return "hots_allocator";
        }

        protected override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("DvznnhhpuH3WkBsUonUytkHhd6MYz91c2iLvRuvLeSnV");
        }

        public async Task<LocationAllocator> GetCurrentState()
        {
            var rawData = await LocationAllocator.GetLocationAllocatorAsync(
                await GetComponentDataAddress(),
                Commitment.Processed);
            if (rawData.ParsedResult == null) return null;

            Debug.Log($"Data:\n {JsonConvert.SerializeObject(rawData.ParsedResult)}");
            return rawData.ParsedResult;
        }

        public async Task<bool> Bump()
        {
            return await ApplySystem(new PublicKey("At6YLXTGPoQPwqzfZwYEq667RBgAX3sJFiNUALgbpQan"), new { });
        }
    }
}