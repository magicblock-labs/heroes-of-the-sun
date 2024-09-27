using Notifications;
using UnityEngine;
using Utils.Injection;

public class PopupManager : InjectableBehaviour
{
    [Inject] private ShowWorkerSelection _showWorkerSelection;

    [SerializeField] private GameObject workerSelectionPopup;

    private void Start()
    {
        _showWorkerSelection.Add(WorkerSelectionRequested);

        foreach (Transform child in transform)
            child.gameObject.SetActive(false);
    }

    private void WorkerSelectionRequested()
    {
        workerSelectionPopup.SetActive(true);
    }
}