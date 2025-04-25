using System;
using Model;
using Notifications;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class PopupManager : InjectableBehaviour
    {
        [Inject] private ShowWorkerSelection _showWorkerSelection;
        [Inject] private ShowResearch _showResearch;
        [Inject] private NavigationContextModel _nav;

        [SerializeField] private GameObject workerSelectionPopup;
        [SerializeField] private GameObject researchPopup;

        private void Start()
        {
            _showWorkerSelection.Add(WorkerSelectionRequested);
            _showResearch.Add(ResearchRequested);

            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
        }

        private void WorkerSelectionRequested()
        {
            workerSelectionPopup.SetActive(true);
        }

        private void ResearchRequested()
        {
            researchPopup.SetActive(true);
        }

        private void Update()
        {
            _nav.IsResearchOpen = researchPopup.activeSelf;
        }

        private void OnDestroy()
        {
            _showWorkerSelection.Remove(WorkerSelectionRequested);
            _showResearch.Remove(ResearchRequested);
        }
    }
}