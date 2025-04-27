using System.Linq;
using Connectors;
using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Injection;

namespace View.UI.Research
{
    public enum ResearchFilter
    {
        None = -1,
        Building,
        Resource,
        Population,
        Faith
    }

    public class ResearchPopup : InjectableBehaviour
    {
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private SettlementModel _settlement;
        [Inject] private ConfigModel _config;
        [Inject] private NavigationContextModel _nav;

        [SerializeField] private Transform researchList;
        [SerializeField] private ResearchItem researchPrefab;

        [SerializeField] private ToggleGroup tabs;

        [SerializeField] private Text selectedResearchTitle;
        [SerializeField] private Text selectedResearchDescription;
        [SerializeField] private Image selectedResearchIcon;
        [SerializeField] private Text selectedResearchCost;

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
            var tabIndex = (ResearchFilter)selectedToggle.transform.GetSiblingIndex();

            if (_nav.ResearchFilter == tabIndex)
                return;

            _nav.ResearchFilter = tabIndex;

            foreach (Transform child in researchList)
                Destroy(child.gameObject);

            var filteredResearch = _config.ResearchTypeMapping
                .Where(x => x.Value == _nav.ResearchFilter)
                .Select(x => x.Key).ToList();

            foreach (var type in filteredResearch)
                Instantiate(researchPrefab, researchList).SetData(type, OnResearchSelected);

            OnResearchSelected(filteredResearch.First());
        }

        void OnResearchSelected(SettlementModel.ResearchType type)
        {
            _nav.SelectedResearch = type;
            selectedResearchTitle.text = type.ToString().SplitCamelCase();
            selectedResearchDescription.text = $"{type}  description".SplitCamelCase();
            selectedResearchIcon.sprite = Resources.Load<Sprite>(type.ToString());
            selectedResearchCost.text = $"x{_settlement.GetResearchCost(type)}";
        }

        public async void OnSubmit()
        {
            await _connector.Research((int)_nav.SelectedResearch);
        }
    }
}