using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Hero.Program;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;

namespace Connectors
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HeroConnector : BaseComponentConnector<Hero.Accounts.Hero>
    {
        protected override Hero.Accounts.Hero DeserialiseBytes(byte[] value)
        {
            return Hero.Accounts.Hero.Deserialize(value);
        }

        protected override TransactionInstruction GetUndelegateIx(PublicKey playerDataPda)
        {
            return HeroProgram.Undelegate(new()
            {
                Payer = Web3.Account,
                DelegatedAccount = playerDataPda
            });
        }

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey(HeroProgram.ID);
        }
        
        public override string GetComponentName()
        {
            return "hero";
        }

        public async UniTask<bool> Move(int x, int y)
        {
            return await ApplySystem("move_hero", new { x, y });
        }
        

        public async UniTask<bool> ChangeBackpack(int food, int wood, int water, int stone, Dictionary<PublicKey, Bolt.Component> extraEntities)
        {
            if (food == 0 && wood == 0 && water == 0 && stone == 0)
                return false;
            
            return await ApplySystem("change_backpack", new {food, wood, water, stone }, extraEntities);
        }

        public override UniTask CloneToRollup()
        {
            return Move(0, 0);//todo use settlement location
        }
    }
}