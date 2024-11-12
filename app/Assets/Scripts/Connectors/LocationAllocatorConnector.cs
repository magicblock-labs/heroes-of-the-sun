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
            return new PublicKey("DvznnhhpuH3WkBsUonUytkHhd6MYz91c2iLvRuvLeSnV");
        }

        protected override LocationAllocator DeserialiseBytes(byte[] value)
        {
            return LocationAllocator.Deserialize(value);
        }
    }
}