using Model;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class DisplayWorkers : InjectableBehaviour
    {
        [Inject] private SettlementModel _settlement;
        [SerializeField] private Worker workerPrefab;

        public void Start()
        {
            _settlement.Updated.Add(Redraw);
            if (_settlement.HasData)
                Redraw();
        }

        private void Redraw()
        {
            var allocation = _settlement.Get().WorkerAssignment;
            for (var i = transform.childCount; i < allocation.Length; i++)
                Instantiate(workerPrefab, transform).SetIndex(i);
        }

        public void OnDestroy()
        {
            _settlement.Updated.Remove(Redraw);
        }
    }
}