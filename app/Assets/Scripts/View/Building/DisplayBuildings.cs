using Model;
using UnityEngine;
using Utils.Injection;
using View.UI.Building;

namespace View.Building
{
    public class DisplayBuildings : InjectableBehaviour, IDisplaySettlementData
    {
        [Inject] private ConfigModel _config;

        [SerializeField] private BuildingPreview prefab;


        public void SetData(global::Settlement.Accounts.Settlement value)
        {
            CreateBuildings(value.Buildings);
        }

        public void CreateBuildings(Settlement.Types.Building[] buildings)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            var i = 0;
            foreach (var building in buildings)
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

                var buildingControls = obj.GetComponent<BuildingControls>();
                if (buildingControls)
                    buildingControls.SetData(i++, building, conf);
            }
        }
    }
}