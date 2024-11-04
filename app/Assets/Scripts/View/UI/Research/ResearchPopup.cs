using System.Linq;
using Connectors;
using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI.Research
{
    public class ResearchPopup : InjectableBehaviour
    {
        [Inject] private SettlementConnector _connector;
        [Inject] private SettlementModel _settlement;
    
        [SerializeField] private Transform researchList;
        [SerializeField] private ResearchItem researchPrefab;

        [SerializeField] private ToggleGroup tabs;

        [SerializeField] private Text selectedResearchTitle;
        [SerializeField] private Text selectedResearchDescription;
        [SerializeField] private Image selectedResearchIcon;
        [SerializeField] private Text selectedResearchCost;

        private readonly SettlementModel.ResearchType[][] _researchGroups =
        {
            new[]
            {
                SettlementModel.ResearchType.BuildingSpeed,
                SettlementModel.ResearchType.BuildingCost,
                SettlementModel.ResearchType.DeteriorationCap
            },
            new[]
            {
                SettlementModel.ResearchType.StorageCapacity,
                SettlementModel.ResearchType.ResourceCollectionSpeed,
                SettlementModel.ResearchType.EnvironmentRegeneration,
                SettlementModel.ResearchType.Mining
            },
            new[]
            {
                SettlementModel.ResearchType.ExtraUnit,
                SettlementModel.ResearchType.DeathTimeout,
                SettlementModel.ResearchType.Consumption
            },
            new[]
            {
                SettlementModel.ResearchType.MaxEnergyCap,
                SettlementModel.ResearchType.EnergyRegeneration,
                SettlementModel.ResearchType.FaithBonus
            }
        };

        private int _selectedTab = -1;
        private SettlementModel.ResearchType _selectedResearch;

        private void Start()
        {
            tabs.EnsureValidState();
            Redraw();
        }

        public void OnTabSelected(int value)
        {
            Redraw();
        }

        private void Redraw()
        {
            var selectedToggle = tabs.ActiveToggles().First();
            var tabIndex = selectedToggle.transform.GetSiblingIndex();

            if (_selectedTab == tabIndex)
                return;

            _selectedTab = tabIndex;
        
            foreach (Transform child in researchList)
                Destroy(child.gameObject);

            foreach (var type in _researchGroups[_selectedTab])
            {
                Instantiate(researchPrefab, researchList).SetData(type, OnResearchSelected);
            }

            OnResearchSelected(_researchGroups[_selectedTab][0]);
        }

        void OnResearchSelected(SettlementModel.ResearchType type)
        {
            _selectedResearch = type;
            selectedResearchTitle.text = type.ToString();
            selectedResearchDescription.text = $"{type}  description";
            selectedResearchIcon.sprite = Resources.Load<Sprite>(type.ToString());
            selectedResearchCost.text = $"x{_settlement.GetResearchCost(type)}";
        }

        public async void OnSubmit()
        {
            if (await _connector.Research((int)_selectedResearch))
                await _connector.ReloadData();
        }
    }
}