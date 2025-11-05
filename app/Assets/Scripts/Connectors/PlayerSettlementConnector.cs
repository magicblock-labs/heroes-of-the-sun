using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Model;
using Notifications;
using Solana.Unity.Rpc.Models;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using Utils;

// ReSharper disable InconsistentNaming

namespace Connectors
{
    [Singleton]
    public class PlayerSettlementConnector : SettlementConnector
    {
        [Inject] private StopFtueSequence _stopFtue;
        [Inject] private TokenConnector _token;
        [Inject] private NextTurnNotification _nextTurn;

        protected override UniTask<bool> ApplySystem(string systemName, object args,
            Dictionary<PublicKey, Bolt.Component> extraEntities = null,
            AccountMeta[] accounts = null, bool forceMainWalletSigner = false)
        {
            _stopFtue.Dispatch();
            return base.ApplySystem(systemName, args, extraEntities, accounts, forceMainWalletSigner);
        }

        public async UniTask<bool> Build(byte x, byte y, byte type, int worker_index)
        {
            return await ApplySystem("build",
                new { x, y, config_index = type, worker_index });
        }

        public async UniTask<bool> Wait(int time)
        {
            var applySystem = await ApplySystem("wait", new { time });

            if (applySystem)
                _nextTurn.Dispatch();
            
            return applySystem;
        }

        public async UniTask<bool> AssignWorker(int worker_index, int building_index)
        {
            return await ApplySystem("assign_worker", new { worker_index, building_index });
        }


        public async UniTask<bool> Repair(int index)
        {
            return await ApplySystem("repair", new { index });
        }

        public async UniTask<bool> Upgrade(int index, int worker_index)
        {
            return await ApplySystem("upgrade", new { index, worker_index });
        }

        public async UniTask<bool> ClaimTime()
        {
            return await base.ApplySystem("claim_time", new { });
        }

        public async UniTask<bool> Research(int research_type)
        {
            return await ApplySystem("research", new { research_type }); //, null, false, _token.GetBurnExtraAccounts());
        }

        public async UniTask<bool> Sacrifice(int index)
        {
            return await ApplySystem("sacrifice", new { index });
        }

        public async UniTask<bool> Reset()
        {
            return await ApplySystem("reset", new { });
        }

        public async UniTask<bool> Exchange(int tokens_for_food, int tokens_for_water, int tokens_for_wood,
            int tokens_for_stone)
        {
            //undelegate
            await Undelegate();

            // if (Web3Utils.SessionWallet != null)
            // {
            //     var tokensTotal = 1000000000 *
            //                       (ulong)(tokens_for_food + tokens_for_water + tokens_for_wood + tokens_for_stone);
            //     var transfer = await Web3.Wallet.Transfer(Web3Utils.SessionWallet.Account.PublicKey,
            //         new PublicKey(TokenConnector.TokenMintPda), tokensTotal);
            //
            //     Debug.Log($"transfer.Result: {transfer.Result}");
            // }

            //2. apply
            var result = await ApplySystem("exchange",
                new { tokens_for_food, tokens_for_water, tokens_for_wood, tokens_for_stone }, null,
                _token.GetBurnExtraAccounts(), true);

            //re-delegate
            await Delegate();

            //claim time (to copy latest state to ER)
            await ClaimTime();

            return result;
        }

        public async UniTask<bool> ClaimQuest(int index)
        {
            return await ApplySystem("claim_quest", new { index });
        }


        public override UniTask CloneToRollup()
        {
            return ClaimTime();
        }
    }
}