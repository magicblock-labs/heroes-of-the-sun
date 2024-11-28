using Model;
using UnityEngine;

namespace View
{
    public class BuildingPreview : MonoBehaviour
    {
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private GameObject constructionSite;
        private BuildingConfig _currentConfig;

        public void ShowConstructionSite()
        {
            _currentConfig = null;

            foreach (Transform child in buildingContainer)
                Destroy(child.gameObject);

            if (constructionSite)
                constructionSite.SetActive(true);
        }

        public void SetBuildingPrefab(BuildingConfig config)
        {
            if (constructionSite)
                constructionSite.SetActive(false);

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
            foreach (Transform child in container)
                Destroy(child.gameObject);

            Instantiate(Resources.Load<GameObject>(config.prefab), container);
        }
    }
}