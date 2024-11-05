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
    public class PlayerConnector : BaseProgramConnector<PlayerClient>
    {
        [Inject] private PlayerModel _model;
        
        private PlayerClient Player =>
            Client ??= new PlayerClient(Web3.Rpc, Web3.WsRpc, new PublicKey(PlayerProgram.ID));
        
        protected override string GetExtraSeed()
        {
            return Web3.Account.PublicKey.Key[..20];
        }

        protected override PublicKey GetComponentProgramAddress()
        {
            return new PublicKey("2JDZnj8f2tTvQhyQtoPrFxcfGJvuunVt9aGG8rDnpkKU");
        }
        
        public async Task ReloadData()
        {
            var rawData = await Player.GetPlayerAsync(
                await GetComponentDataAddress(), 
                Commitment.Processed);
            if (rawData.ParsedResult == null) return;

            Debug.Log($"Data:\n {JsonConvert.SerializeObject(rawData.ParsedResult)}");
            
            _model.Set(rawData.ParsedResult);
        }
        
        public async Task<bool> AssignSettlement(short location_x, short location_y)
        {
            return await ApplySystem(new PublicKey("GgTkjpRSLRFmw27h7uCD9Bh1MWSvKv5aPxkMjko3mxWp"),
                new { location_x, location_y });
        }
    }
}