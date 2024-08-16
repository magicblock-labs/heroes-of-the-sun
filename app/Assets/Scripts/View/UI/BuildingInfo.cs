using Model;
using Service;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class BuildingInfo : BuildingUIPanel
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;

        [SerializeField] private Text nameLabel;
        [SerializeField] private Text levelLabel;
        [SerializeField] private Text deteriorationLabel;
        private int _index;

        public void SetData(int index, Building value, BuildingConfig config)
        {
            if (value == null)
                return;

            _index = index;

            nameLabel.text = value.Id.ToString();
            if (levelLabel)
                levelLabel.text = "Level: " + value.Level;

            if (deteriorationLabel)
                deteriorationLabel.text = "Deterioration: " + value.Deterioration;
        }

        public async void Repair()
        {
            if (await _connector.Repair(_index))
                await _connector.ReloadData();
        }

        public async void Upgrade()
        {
            if (await _connector.Upgrade(_index))
                await _connector.ReloadData();
        }
    }
}