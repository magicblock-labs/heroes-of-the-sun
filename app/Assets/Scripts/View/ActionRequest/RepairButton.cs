using System;
using Connectors;
using Model;
using Notifications;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;
using View.UI.Building;

namespace View.ActionRequest
{
    public class RepairButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private ConfigModel _config;
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private ResourceDiffNotification _resourceDiff;

        [SerializeField] private GameObject costWood;
        [SerializeField] private Text costWoodLabel;

        [SerializeField] private GameObject costStone;
        [SerializeField] private Text costStoneLabel;

        private int _index;
        private bool _canAfford;
        private Action _callback;
        private ResourceBalance _cost;

        public void SetData(int index, Settlement.Types.Building value, Action callback)
        {
            if (value == null)
                return;


            _callback = callback;

            _index = index;

            gameObject.SetActive(value.Deterioration > 0);

            if (value.Deterioration == 0)
                return;

            var relativeDeterioration = value.Deterioration / _settlement.GetMaxDeterioration();

            var treasury = _settlement.Get().Treasury;
            _cost = _settlement.GetConstructionCost(_config.Buildings[value.Id].costTier, value.Level + 1,
                relativeDeterioration);

            _canAfford = true;

            costWood.SetActive(_cost.Wood > 0);
            costWoodLabel.text = _cost.Wood.ToString();
            costWoodLabel.color = _cost.Wood <= treasury.Wood ? Color.white : Color.red;
            _canAfford &= _cost.Wood <= treasury.Wood;

            costStone.SetActive(_cost.Stone > 0);
            costStoneLabel.text = _cost.Stone.ToString();
            costStoneLabel.color = _cost.Stone <= treasury.Stone ? Color.white : Color.red;
            _canAfford &= _cost.Stone <= treasury.Stone;
        }

        public async void Repair()
        {
            if (!_canAfford) return;

            _callback?.Invoke();
            _gridInteraction.LockInteraction();

            _resourceDiff.Dispatch(new ResourceDiff()
            {
                Wood = -_cost.Wood,
                Stone = -_cost.Stone,
            }, 0, transform.GetComponentInParent<AnchoredUIPanel>().worldAnchor);
                
            await _connector.Repair(_index);
        }
    }
}