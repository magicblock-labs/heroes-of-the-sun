using Model;
using Service;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class DisplayLabourAllocation : InjectableBehaviour
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;

        [SerializeField] private LabourAllocationEntry labourAllocationEntry;
        private int _buildingIndex = -1;

        public void Start()
        {
            _settlement.Updated.Add(Redraw);
            if (_settlement.HasData)
                Redraw();
        }

        public void SetBuildingIndex(int value)
        {
            _buildingIndex = value;
        }

        private void Redraw()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var allocation = _settlement.Get().LabourAllocation;
            for (var i = 0; i < allocation.Length; i++)
            {
                var allocationEntry = Instantiate(labourAllocationEntry, transform);
                allocationEntry.SetData(i, allocation[i]);
                if (_buildingIndex >= 0)
                    allocationEntry.onSelected.AddListener(TryAssignLabour);
            }
        }

        private async void TryAssignLabour(int index)
        {
            if (await _connector.AssignLabour(index, _buildingIndex))
                await _connector.ReloadData();
        }

        public void OnDestroy()
        {
            _settlement.Updated.Remove(Redraw);
        }
    }
}