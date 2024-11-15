using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Model;
using Solana.Unity.Wallet;
using Utils.Injection;

namespace Connectors
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SettlementConnector : BaseComponentConnector<Settlement.Accounts.Settlement>
    {
        [Inject] private SettlementModel _settlement;

        public override PublicKey GetComponentProgramAddress()
        {
            return new("B2h45ZJwpiuD9jBY7Dfjky7AmEzdzGsty4qWQxjX9ycv");
        }

        protected override Settlement.Accounts.Settlement DeserialiseBytes(byte[] value)
        {
            return Settlement.Accounts.Settlement.Deserialize(value);
        }
    }
}