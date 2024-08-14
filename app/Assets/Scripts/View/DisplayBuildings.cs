using Model;
using Service;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class DisplayBuildings : InjectableBehaviour
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
            
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            foreach (var building in _model.Get().Buildings)
            {
                var conf = _config.Buildings[(BuildingType)building.Id];
                var buildingDimensions = new Vector3(conf.width, 0, conf.height);
                
                var centerPos = (new Vector3(building.X, 0, building.Y) + buildingDimensions / 2) *
                            DisplayPlacementPreview.CellSize;

                var obj = Instantiate(prefab, transform);
                obj.SetBuildingPrefab(conf);
                obj.transform.localPosition = centerPos;
                var info = obj.GetComponentInChildren<BuildingInfo>();
                if (info)
                    info.SetData(building, conf);
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnModelUpdated);
        }
    }
}