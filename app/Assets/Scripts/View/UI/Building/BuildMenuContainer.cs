using Model;
using UnityEngine;
using Utils.Injection;

namespace View.UI.Building
{
    public class BuildMenuContainer : InjectableBehaviour
    {
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private NavigationContextModel _nav;

        [SerializeField] private GameObject cta;
        [SerializeField] private GameObject menu;

        private void Start()
        {
            _gridInteraction.Updated.Add(OnInteractionUpdated);
            _nav.Updated.Add(OnNavigationUpdated);
            _nav.IsBuildMenuOpen = false;
        }

        private void OnInteractionUpdated()
        {
            if (_gridInteraction.SelectedBuildingType.HasValue)
                _nav.IsBuildMenuOpen = false;
        }
        
        private void OnNavigationUpdated()
        {
            cta.SetActive(!_nav.IsBuildMenuOpen);
            menu.SetActive(_nav.IsBuildMenuOpen);
        }

        public void OnCloseClick()
        {
            _nav.IsBuildMenuOpen = false;
        }

        public void OnCtaClick()
        {
            _gridInteraction.FinishPlacement();
            _nav.IsBuildMenuOpen = true;
        }

        private void OnDestroy()
        {
            _nav.IsBuildMenuOpen = false;
            _gridInteraction.Updated.Remove(OnInteractionUpdated);
            _nav.Updated.Add(OnNavigationUpdated);
        }
    }
}