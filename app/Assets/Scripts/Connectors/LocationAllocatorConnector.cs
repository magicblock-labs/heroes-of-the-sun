using Locationallocator.Accounts;
using Locationallocator.Program;
using Solana.Unity.Wallet;
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

        protected override LocationAllocator DeserialiseBytes(byte[] value)
        {
            return LocationAllocator.Deserialize(value);
        }
    }
}