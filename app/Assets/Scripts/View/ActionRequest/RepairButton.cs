using System;
using Connectors;
using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.ActionRequest
{
    public class RepairButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private ConfigModel _config;
        [Inject] private GridInteractionStateModel _gridInteraction;

        [SerializeField] private GameObject costWood;
        [SerializeField] private Text costWoodLabel;

        [SerializeField] private GameObject costStone;
        [SerializeField] private Text costStoneLabel;

        private int _index;
        private bool _canAfford;
        private Action _callback;

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
            var cost = _settlement.GetConstructionCost(_config.Buildings[value.Id].costTier, value.Level + 1,
                relativeDeterioration);

            _canAfford = true;

            costWood.SetActive(cost.Wood > 0);
            costWoodLabel.text = cost.Wood.ToString();
            costWoodLabel.color = cost.Wood <= treasury.Wood ? Color.white : Color.red;
            _canAfford &= cost.Wood <= treasury.Wood;

            costStone.SetActive(cost.Stone > 0);
            costStoneLabel.text = cost.Stone.ToString();
            costStoneLabel.color = cost.Stone <= treasury.Stone ? Color.white : Color.red;
            _canAfford &= cost.Wood <= treasury.Wood;
        }

        public async void Repair()
        {
            _callback?.Invoke();

            _gridInteraction.LockInteraction();

            if (_canAfford)
                await _connector.Repair(_index);
        }
    }
}