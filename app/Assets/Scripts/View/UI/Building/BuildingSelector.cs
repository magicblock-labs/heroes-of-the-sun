using System;
using Model;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI.Building
{
    public class BuildingSelector : InjectableBehaviour
    {
        [Inject] private ConfigModel _config;
        [Inject] private SettlementModel _model;
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private BuildingSnapshot snapshot;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text costLabel;

        private Action<BuildingType> _selectedCallback;
        private BuildingType _type;

        public void SetData(BuildingType value)
        {
            _type = value;
            var buildingConfig = _config.Buildings[value];

            snapshot.Generate(buildingConfig);
            nameLabel.text = value.ToString();
            costLabel.text = buildingConfig.cost.ToString();
        }

        public void OnClick()
        {
            _interaction.StartPlacement(_type);
        }
    }
}