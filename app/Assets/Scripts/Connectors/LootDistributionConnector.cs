using System.Collections.Generic;
using System.Threading.Tasks;
using Locationallocator.Accounts;
using Locationallocator.Program;
using Lootdistribution.Accounts;
using Solana.Unity.Wallet;
using Utils.Injection;

namespace Connectors
{
    [Singleton]
    public class LootDistributionConnector : BaseComponentConnector<LootDistribution>
    {
        public const string DefaultSeed = "hots_loot_distribution";

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("5F9tMTcNhgjL3tWCaF5HwLkQP9z4XJ4nTXmbYeS8UXRW");
        }

        protected override LootDistribution DeserialiseBytes(byte[] value)
        {
            return LootDistribution.Deserialize(value);
        }

        public async Task<bool> Claim(int index, Dictionary<PublicKey, PublicKey> extraEntities)
        {
            return await ApplySystem(new PublicKey("4CjxHvNUpoCYomULBFTvmkTQPaNd9QDHPhZQ6eB9bZEf"),
                new { index }, extraEntities);
        }
    }
}