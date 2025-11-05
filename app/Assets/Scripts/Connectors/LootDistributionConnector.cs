using System.Threading.Tasks;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils;
using Utils.Injection;

namespace Connectors
{
    [Singleton]
    public class LootDistributionConnector : BaseComponentConnector<LootDistribution.Accounts.LootDistribution>
    {
        public const string DefaultSeed = "hots_loot_distribution";
        [Inject] private TokenConnector _token;

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("5F9tMTcNhgjL3tWCaF5HwLkQP9z4XJ4nTXmbYeS8UXRW");
        }

        public override string GetComponentName()
        {
            return "loot_distribution";
        }
        
        protected override LootDistribution.Accounts.LootDistribution DeserialiseBytes(byte[] value)
        {
            return LootDistribution.Accounts.LootDistribution.Deserialize(value);
        }

        protected override TransactionInstruction GetUndelegateIx(PublicKey playerDataPda)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> Claim(int index)
        {
            var applySystem = await ApplySystem("claim_loot", new { index }, null, _token.GetMintExtraAccounts());


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