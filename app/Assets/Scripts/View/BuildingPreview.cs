using Service;
using UnityEngine;

namespace View
{
    public class BuildingPreview : MonoBehaviour
    {
        [SerializeField] private Transform buildingContainer;
        private BuildingConfig _currentConfig;

        public void SetBuildingPrefab(BuildingConfig config)
        {
            if (_currentConfig == config)
                return;

            _currentConfig = config;

            foreach (Transform child in buildingContainer)
                Destroy(child.gameObject);

            CreateBuildingInto(config, buildingContainer);
        }

        public void SetMaterialOverride(Material value)
        {
            foreach (var mesh in buildingContainer.GetComponentsInChildren<MeshRenderer>())
            {
                var mats = mesh.materials;
                for (var i = 0; i < mats.Length; i++)
                    mats[i] = value;
                mesh.materials = mats;
            }
        }

        public static void CreateBuildingInto(BuildingConfig config, Transform container)
        {
            Instantiate(Resources.Load<GameObject>(config.prefab), container);
        }
    }
}