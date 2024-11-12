using Connectors;
using Model;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.ActionRequest
{
    public class RepairButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private SettlementConnector _connector;
        [Inject] private ConfigModel _config;
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private GameObject costWood;
        [SerializeField] private Text costWoodLabel;

        [SerializeField] private GameObject costStone;
        [SerializeField] private Text costStoneLabel;

        [SerializeField] private GameObject costGold;
        [SerializeField] private Text costGoldLabel;

        private int _index;
        private bool _canAfford;

        public void SetData(int index, Building value)
        {
            if (value == null)
                return;

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

            costGold.SetActive(cost.Gold > 0);
            costGoldLabel.text = cost.Gold.ToString();
            costGoldLabel.color = cost.Gold <= treasury.Gold ? Color.white : Color.red;
            _canAfford &= cost.Wood <= treasury.Wood;
        }

        public async void Repair()
        {
            _interaction.LockInteraction();

            if (_canAfford)
                await _connector.Repair(_index);
        }
    }
}