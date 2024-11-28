using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Player;
using Player.Program;
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
            return new PublicKey("2JDZnj8f2tTvQhyQtoPrFxcfGJvuunVt9aGG8rDnpkKU");
        }

        protected override Player.Accounts.Player DeserialiseBytes(byte[] value)
        {
            return Player.Accounts.Player.Deserialize(value);
        }

        public async Task<bool> AssignSettlement(Dictionary<PublicKey, PublicKey> extraEntities)
        {
            return await ApplySystem(new PublicKey("42g6wojVK214btG2oUHg8vziW8UaUiQfPZ6K9kMGTCp2"),
                new { }, extraEntities);
        }


        public async Task<bool> AssignHero(Dictionary<PublicKey, PublicKey> extraEntities)
        {
            return await ApplySystem(new PublicKey("7gBLDn72Cog7dBvN1LWfo6W36Q7vxcv7CqYAeHwfo3Y"),
                new { }, extraEntities);
        }
    }
}