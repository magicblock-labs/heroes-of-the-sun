using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Player;
using Player.Program;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;


namespace Connectors
{
    [Singleton]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PlayerConnector : BaseComponentConnector<Player.Accounts.Player>
    {
        [Inject] private PlayerModel _model;

        public override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("FDY4hyNT9yaV3oXowH7u4guB2gW3Aj8psvLnGwQ9BuT6");
        }

        public override string GetComponentName()
        {
            return "player";
        }

        protected override Player.Accounts.Player DeserialiseBytes(byte[] value)
        {
            return Player.Accounts.Player.Deserialize(value);
        }

        protected override TransactionInstruction GetUndelegateIx(PublicKey playerDataPda)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AssignSettlement(Dictionary<PublicKey, Bolt.Component> extraEntities)
        {
            return await ApplySystem("assign_settlement", new { }, extraEntities);
        }


        public async Task<bool> AssignHero(Dictionary<PublicKey, Bolt.Component> extraEntities)
        {
            return await ApplySystem("assign_hero", new { }, extraEntities);
        }
    }
}