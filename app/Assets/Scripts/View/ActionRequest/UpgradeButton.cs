using Model;
using Service;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.ActionRequest
{
    public class UpgradeButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;
        [Inject] private ConfigModel _config;

        [SerializeField] private Text costLabel;

        private int _index;
        private bool _canAfford;

        public void SetData(int index, Building value)
        {
            if (value == null)
                return;

            _index = index;

            var cost = _config.Buildings[value.Id].cost;
            _canAfford = cost <= _settlement.Get().Treasury.Wood;
            costLabel.text = _canAfford ? $"{cost}" : $"<color=red>{cost}</color>";
        }

        public async void Upgrade()
        {
            if (_canAfford && await _connector.Upgrade(_index))
                await _connector.ReloadData();
        }
    }
}