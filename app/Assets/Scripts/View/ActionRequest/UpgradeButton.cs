using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Notifications;
using Plugins.Demigiant.DOTween.Modules;
using Service;
using Settlement.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.UI
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