using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Hero.Program;
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

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey(HeroProgram.ID);
        }

        public async UniTask<bool> Move(int x, int y)
        {
            return await ApplySystem(new PublicKey("6o9i5V3EvT9oaokbcZa7G92DWHxcqJnjXmCp94xxhQhv"),
                new { x, y }, null, true);
        }
        

        public async UniTask<bool> ChangeBackpack(int food, int wood, int water, int stone, Dictionary<PublicKey, PublicKey> extraEntities)
        {
            if (food == 0 && wood == 0 && water == 0 && stone == 0)
                return false;
            
            return await ApplySystem(new PublicKey("97qK4zBtZbSGT1mSw5mn12hfHgz4jV4C7cLmwSzH2eua"),
                new {food, wood, water, stone }, extraEntities);
        }

        public override UniTask CloneToRollup()
        {
            return Move(0, 0);//todo use settlement location
        }
    }
}