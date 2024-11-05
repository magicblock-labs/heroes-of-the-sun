using Model;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class ApplyOwnSettlementData : InjectableBehaviour
    {
        [Inject] private SettlementModel _model;
        [Inject] private ConfigModel _config;

        [SerializeField] private BuildingPreview prefab;

        private void Start()
        {
            _model.Updated.Add(OnModelUpdated);
            OnModelUpdated();
        }

        private void OnModelUpdated()
        {
            if (!_model.HasData)
                return;

            foreach (var settlementDataRenderer in GetComponentsInChildren<IDisplaySettlementData>())
                settlementDataRenderer.SetData(_model.Get());
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnModelUpdated);
        }
    }
}