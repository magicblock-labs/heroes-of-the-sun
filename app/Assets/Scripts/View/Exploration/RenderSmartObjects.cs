using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connectors;
using Model;
using Notifications;
using Solana.Unity.Rpc.Types;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class RenderSmartObjects : InjectableBehaviour
    {
        [Inject] private PathfindingModel _pathfinding;
        [Inject] private SmartObjectLocationConnector _connector;

        [Inject] private RedrawSmartObjects _redraw;

        [SerializeField] private RenderSmartObject prefab;

        private Dictionary<string, RenderSmartObject> _cache = new();

        private void Start()
        {
            _ = Redraw();
            _redraw.Add(OnRedrawRequest);
        }

        private void OnRedrawRequest()
        {
            _ = Redraw();
        }

        private async Task Redraw()
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>
            {
                new() { Bytes = SmartObjectLocation.Accounts.SmartObjectLocation.ACCOUNT_DISCRIMINATOR_B58, Offset = 0 }
            };

            var accounts = (await Web3.Rpc.GetProgramAccountsAsync(
                _connector.GetComponentProgramAddress(), Commitment.Confirmed, memCmpList: list)).Result;

            foreach (var account in accounts)
            {
                if (!_cache.ContainsKey(account.PublicKey))
                {
                    var renderSmartObject = Instantiate(prefab, transform);
                    await renderSmartObject.SetDataAddress(account.PublicKey);
                    _cache[account.PublicKey] = renderSmartObject;
                }
                else
                {
                    _cache[account.PublicKey].UpdateData();
                }
            }
        }

        private void OnDestroy()
        {
            _redraw.Remove(OnRedrawRequest);
        }
    }
}