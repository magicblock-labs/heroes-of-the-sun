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
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private CtaRegister _ctaRegister;

        [SerializeField] private BuildingSnapshot snapshot;
        [SerializeField] private Text nameLabel;

        [SerializeField] private GameObject costWood;
        [SerializeField] private Text costWoodLabel;

        [SerializeField] private GameObject costStone;
        [SerializeField] private Text costStoneLabel;

        private Action<BuildingType> _selectedCallback;
        private BuildingType? _type;

        private void Start()
        {
            if (_type.HasValue)
                _ctaRegister.Add(transform, CtaTag.BuildMenuBuilding, _type);
        }

        public void SetData(BuildingType value)
        {
            _type = value;
            var buildingConfig = _config.Buildings[value];

            snapshot.Generate(buildingConfig);
            nameLabel.text = value.ToString();

            var treasury = _model.Get().Treasury;
            var cost = _model.GetConstructionCost(buildingConfig.costTier, 1, 1);

            costWood.SetActive(cost.Wood > 0);
            costWoodLabel.text = cost.Wood.ToString();
            costWoodLabel.color = cost.Wood <= treasury.Wood ? Color.white : Color.red;

            costStone.SetActive(cost.Stone > 0);
            costStoneLabel.text = cost.Stone.ToString();
            costStoneLabel.color = cost.Stone <= treasury.Stone ? Color.white : Color.red;
        }

        public void OnClick()
        {
            if (_type.HasValue)
                _gridInteraction.StartPlacement(_type.Value);
        }

        private void OnDestroy()
        {
            if (_type.HasValue)
                _ctaRegister.Remove(CtaTag.BuildMenuBuilding, _type.Value);
        }
    }
}