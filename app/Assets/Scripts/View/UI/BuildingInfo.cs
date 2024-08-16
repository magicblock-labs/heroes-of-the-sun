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

        [SerializeField] private Text nameLabel;
        [SerializeField] private Text levelLabel;
        [SerializeField] private Text deteriorationLabel;

        public void SetData(Building value, BuildingConfig config)
        {
            if (value == null)
                return;

            nameLabel.text = value.Id.ToString();
            if (levelLabel)
                levelLabel.text = "Level: " + value.Level;

            if (deteriorationLabel)
                deteriorationLabel.text = "Deterioration: " + value.Deterioration;
        }
    }
}