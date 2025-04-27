using System.Collections.Generic;
using Connectors;
using Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utils.Injection;

namespace View.UI
{
    public class DisplayWorkerAssignment : InjectableBehaviour
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private GridInteractionStateModel _gridInteraction;

        [SerializeField] private WorkerAssignmentEntry workerAssignmentEntry;
        [SerializeField] private UnityEvent close;

        private void OnEnable()
        {
            if (!_settlement.HasData)
                return;

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var allocation = _settlement.Get().WorkerAssignment;

            //group by building ID
            var grouped = new Dictionary<sbyte, List<int>>();

            for (var i = 0; i < allocation.Length; i++)
            {
                if (!grouped.ContainsKey(allocation[i]))
                    grouped[allocation[i]] = new List<int>();

                grouped[allocation[i]].Add(i);
            }

            foreach (var (buildingId, workerIds) in grouped)
            {
                if (buildingId < 0)
                    continue; //skip dead workers

                var allocationEntry = Instantiate(workerAssignmentEntry, transform);
                allocationEntry.SetData(buildingId, workerIds);
                allocationEntry.onSelected.AddListener(TryAssignLabour);
            }
        }

        private async void TryAssignLabour(int index)
        {
            close?.Invoke();

            if (_gridInteraction.SelectedBuildingIndex < 0) return;
            _gridInteraction.LockInteraction();
            await _connector.AssignWorker(index, _gridInteraction.SelectedBuildingIndex);
        }
    }
}