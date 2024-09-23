using Model;
using Service;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class BuildMenuContainer : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private GameObject cta;
        [SerializeField] private GameObject menu;

        private void Start()
        {
            _interaction.Updated.Add(OnInteractionUpdated);
            ShowMenu(false);
        }

        private void OnInteractionUpdated()
        {
            if (_interaction.SelectedBuildingType != BuildingType.None)
                ShowMenu(false);
        }

        public void OnCloseClick()
        {
            ShowMenu(false);
        }

        public void OnCtaClick()
        {
            _interaction.FinishPlacement();
            ShowMenu(true);
        }

        private void ShowMenu(bool value)
        {
            cta.SetActive(!value);
            menu.SetActive(value);
        }

        private void OnDestroy()
        {
            _interaction.Updated.Remove(OnInteractionUpdated);
        }
    }
}