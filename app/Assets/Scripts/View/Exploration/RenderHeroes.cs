using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connectors;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using UnityEngine;
using Utils;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderHeroes : InjectableBehaviour
    {
        [Inject] private HeroConnector _connector;
        [SerializeField] private RenderHero prefab;

        void Start()
        {
            _ = Init();
        }

        private async Task Init()
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>
                { new() { Bytes = Hero.Accounts.Hero.ACCOUNT_DISCRIMINATOR_B58, Offset = 0 } };

            var accounts = (await Web3Utils.EphemeralWallet.ActiveRpcClient.GetProgramAccountsAsync(
                _connector.GetComponentProgramAddress(), Commitment.Confirmed, memCmpList: list)).Result;

            //concat with non-rollup accounts?
            foreach (var account in accounts)
            {
                try
                {
                    var renderHero = Instantiate(prefab, transform);
                    await renderHero.SetDataAddress(account.PublicKey);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}