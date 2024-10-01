using System;
using Model;
using Service;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.ActionRequest
{
    public class RepairButton : InjectableBehaviour, IBuildingActionButton
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

            var cost = Math.Ceiling(_config.Buildings[value.Id].cost * (float)value.Deterioration / 127);
            _canAfford = cost <= _settlement.Get().Treasury.Wood;
            
            costLabel.text =  _canAfford ? $"{cost}" : $"<color=red>{cost}</color>";
            
        }

        public async void Repair()
        {
            if (_canAfford && await _connector.Repair(_index))
                await _connector.ReloadData();
        }

    }
}