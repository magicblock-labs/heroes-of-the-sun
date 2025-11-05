using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Wallet;
using UnityEngine;

namespace Connectors
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class SmartObjectLocationConnector : BaseComponentConnector<SmartObjectLocation.Accounts.SmartObjectLocation>
    {
        protected override SmartObjectLocation.Accounts.SmartObjectLocation DeserialiseBytes(byte[] value)
        {
            var encoded = System.Convert.ToBase64String(value);
            PlayerPrefs.SetString(DataAddress, encoded);
            return SmartObjectLocation.Accounts.SmartObjectLocation.Deserialize(value);
        }

        protected override TransactionInstruction GetUndelegateIx(PublicKey playerDataPda)
        {
            throw new System.NotImplementedException();
        }

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("5ewDDvpaTkYvoE7ZJJ9cDmZuqvGQt65hsZSJ9w73Fzr1");
        }
        
        
        
        public override string GetComponentName()
        {
            return "smart_object_location";
        }

        public async Task<bool> Init(int x, int y)
        {
            var entity = new PublicKey(EntityPda).KeyBytes.Select(b => (int)b).ToArray();
            return await ApplySystem("smart_object_init", new { x, y, entity });
        }
    }
}