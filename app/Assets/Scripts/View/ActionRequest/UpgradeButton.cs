using System.Threading.Tasks;
using Connectors;
using Model;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.ActionRequest
{
    public class UpgradeButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private ConfigModel _config;
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private GameObject costWood;
        [SerializeField] private Text costWoodLabel;

        [SerializeField] private GameObject costStone;
        [SerializeField] private Text costStoneLabel;

        private int _index;
        private bool _canAfford;

        public void SetData(int index, Settlement.Types.Building value)
        {
            if (value == null)
                return;

            var reachedTownHallLevel =
                value.Id != BuildingType.TownHall && _settlement.Get().Buildings[0].Level <= value.Level;
            gameObject.SetActive(!reachedTownHallLevel);
            if (reachedTownHallLevel)
                return;

            _index = index;
            _canAfford = true;

            var treasury = _settlement.Get().Treasury;
            var cost = _settlement.GetConstructionCost(_config.Buildings[value.Id].costTier, value.Level + 1, 1);

            costWood.SetActive(cost.Wood > 0);
            costWoodLabel.text = cost.Wood.ToString();
            costWoodLabel.color = cost.Wood <= treasury.Wood ? Color.white : Color.red;
            _canAfford &= cost.Wood <= treasury.Wood;

            costStone.SetActive(cost.Stone > 0);
            costStoneLabel.text = cost.Stone.ToString();
            costStoneLabel.color = cost.Stone <= treasury.Stone ? Color.white : Color.red;
            _canAfford &= cost.Wood <= treasury.Wood;
        }

        public void Upgrade()
        {
            _interaction.LockInteraction();

            if (_canAfford)
                _ = _connector.Upgrade(_index, _settlement.GetFreeWorkerIndex());
        }
    }
}