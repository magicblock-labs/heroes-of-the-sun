using System.Collections.Generic;
using System.Threading.Tasks;
using Lootdistribution.Accounts;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils;
using Utils.Injection;

namespace Connectors
{
    [Singleton]
    public class LootDistributionConnector : BaseComponentConnector<LootDistribution>
    {
        public const string DefaultSeed = "hots_loot_distribution";
        [Inject] private TokenConnector _token;

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
            var applySystem = await ApplySystem(new PublicKey("4CjxHvNUpoCYomULBFTvmkTQPaNd9QDHPhZQ6eB9bZEf"),
                new { index }, null, false, _token.GetMintExtraAccounts());


            if (applySystem && Web3Utils.SessionWallet != null)
            {
                var transfer = await Web3Utils.SessionWallet.Transfer(Web3.Account.PublicKey,
                    new PublicKey(TokenConnector.TokenMintPda), 1000000000);
                
                Debug.Log($"transfer.Result: {transfer.Result}");
            }

            return applySystem;
        }
    }
}