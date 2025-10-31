using System.Threading.Tasks;
using Locationallocator.Accounts;
using Locationallocator.Program;
using Newtonsoft.Json;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;

namespace Connectors
{
    [Singleton]
    public class LocationAllocatorConnector : BaseComponentConnector<LocationAllocator>
    {
        public const string DefaultSeed = "hots_allocator";

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("J7q3dEg2KauPKkMamH9Q5FHhCoFYsSq9ramdutMpPTDc");
        }

        public override string GetComponentName()
        {
            return "location_allocator";
        }

        protected override LocationAllocator DeserialiseBytes(byte[] value)
        {
            return LocationAllocator.Deserialize(value);
        }

        protected override TransactionInstruction GetUndelegateIx(PublicKey playerDataPda)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> GetNextUnallocatedLocation()
        {
            while (true)
            {
                var currentState = await LoadData();
                var extraSeed = $"{currentState.CurrentX}_{currentState.CurrentY}";
                
                Debug.Log($"[Allocator state]: {extraSeed}");
                
                var settlementEntity = Pda.FindEntityPda(WorldIndex, 0, extraSeed);

                var entityState = await RpcClient.GetAccountInfoAsync(settlementEntity);
                if (entityState.Result.Value == null)
                {
                    return extraSeed;
                }

                await Bump();
            }
        }

        private async Task<bool> Bump()
        {
            return await ApplySystem("bump_location_allocator", new { });
        }
    }
}