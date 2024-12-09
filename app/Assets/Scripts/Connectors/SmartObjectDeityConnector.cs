using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Smartobjectdeity.Accounts;
using Smartobjectlocation.Accounts;
using Solana.Unity.Wallet;

namespace Connectors
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SmartObjectDeityConnector : BaseComponentConnector<SmartObjectDeity>
    {
        protected override SmartObjectDeity DeserialiseBytes(byte[] value)
        {
            return SmartObjectDeity.Deserialize(value);
        }

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("9RfzWgEBYQAM64a46V3dGRPKYsVY8a7YvZszWPMxvBfk");
        }

        public async Task<bool> Interact(int index, PublicKey systemAddress,
            Dictionary<PublicKey, PublicKey> extraEntities)
        {
            return await ApplySystem(systemAddress, new { index }, extraEntities);
        }
    }
}