using Model;
using Service;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utils.Injection;

namespace View.UI
{
    public class DisplayWorkerAssignment : InjectableBehaviour
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private WorkerAssignmentEntry workerAssignmentEntry;
        [SerializeField] private UnityEvent close;

        private void OnEnable()
        {
            if (!_settlement.HasData)
                return;
            
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var allocation = _settlement.Get().WorkerAssignment;
            for (var i = 0; i < allocation.Length; i++)
            {
                var allocationEntry = Instantiate(workerAssignmentEntry, transform);
                allocationEntry.SetData(i, allocation[i]);
                allocationEntry.onSelected.AddListener(TryAssignLabour);
            }
        }

        private async void TryAssignLabour(int index)
        {
            close?.Invoke();
            
            if (_interaction.SelectedBuildingIndex < 0) return;
            if (await _connector.AssignLabour(index, _interaction.SelectedBuildingIndex))
                await _connector.ReloadData();
            
        }
    }
}