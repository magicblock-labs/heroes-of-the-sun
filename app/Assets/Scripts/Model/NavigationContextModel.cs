using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;
using View.UI.Building;
using View.UI.Research;

namespace Model
{
    [Singleton]
    public class NavigationContextModel
    {
        public Signal Updated = new();

        private bool _isBuildMenuOpen;
        public bool IsBuildMenuOpen
        {
            get => _isBuildMenuOpen;
            set
            {
                if (_isBuildMenuOpen == value) return;
                _isBuildMenuOpen = value;
                Updated.Dispatch();
            }
        }

        public bool IsResearchOpen;
        public BuildingFilter BuildingFilter;
        public ResearchFilter ResearchFilter = ResearchFilter.None;
        public SettlementModel.ResearchType SelectedResearch;

    }
}