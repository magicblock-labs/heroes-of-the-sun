using Model;
using Service;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using View.UI;
using View.UI.Building;

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

            var i = 0;
            foreach (var building in _model.Get().Buildings)
            {
                var conf = _config.Buildings[building.Id];
                var buildingDimensions = new Vector3(conf.width, 0, conf.height);

                var centerPos = (new Vector3(building.X, 0, building.Y) + buildingDimensions / 2) *
                                ConfigModel.CellSize;

                var obj = Instantiate(prefab, transform);

                if (building.TurnsToBuild > 0)
                    obj.ShowConstructionSite();
                else
                    obj.SetBuildingPrefab(conf);
                    
                obj.transform.localPosition = centerPos;

                obj.GetComponent<BuildingControls>().SetData(i++, building, conf);
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnModelUpdated);
        }
    }
}