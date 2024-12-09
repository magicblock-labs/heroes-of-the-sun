using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Smartobjectlocation.Accounts;
using Solana.Unity.Wallet;

namespace Connectors
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SmartObjectLocationConnector : BaseComponentConnector<SmartObjectLocation>
    {
        protected override SmartObjectLocation DeserialiseBytes(byte[] value)
        {
            return SmartObjectLocation.Deserialize(value);
        }

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("5ewDDvpaTkYvoE7ZJJ9cDmZuqvGQt65hsZSJ9w73Fzr1");
        }

        public async Task<bool> Init(int x, int y)
        {
            var entity = new PublicKey(EntityPda).KeyBytes;
            return await ApplySystem(new PublicKey("64Uk4oF6mNyviUdK2xHXE3VMCtbCMDgRr1DMJk777DJZ"),
                new { x, y, entity }, null, true);
        }
    }
}