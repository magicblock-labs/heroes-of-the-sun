using UnityEngine;
using Utils.Injection;

namespace View.Building
{
    public class DisplayWorkers : InjectableBehaviour, IDisplaySettlementData
    {
        [SerializeField] private Worker workerPrefab;

        public void SetData(global::Settlement.Accounts.Settlement value)
        {
            if (value == null)
                return;
            
            var allocation = value.WorkerAssignment;
            for (var i = 0; i < allocation.Length; i++)
            {
                if (i < transform.childCount)
                    transform.GetChild(i).GetComponent<Worker>().ResetLogic();
                else
                    Instantiate(workerPrefab, transform).SetIndex(i);
            }
        }
    }
}