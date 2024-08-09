using Service;
using UnityEngine;

namespace View
{
    public class BuildingPreview : MonoBehaviour
    {
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private Material materialOverride;

        public void SetBuildingPrefab(BuildingConfig config)
        {
            foreach (Transform child in buildingContainer)
                Destroy(child.gameObject);

            var obj = GenerateBuilding(config, buildingContainer);

            if (materialOverride == null) return;

            foreach (var mesh in obj.GetComponentsInChildren<MeshRenderer>())
            {
                Material[] mats = mesh.materials;
                for (var i = 0; i < mats.Length; i++)
                    mats[i] = materialOverride;
                mesh.materials = mats;
            }
        }

        public static GameObject GenerateBuilding(BuildingConfig config, Transform parent)
        {
            var generate = string.IsNullOrEmpty(config.prefab);
            var obj = generate
                ? new GameObject("Building")
                : Instantiate(Resources.Load<GameObject>(config.prefab), parent);


            if (generate)
            {
                var objTransform = obj.transform;
                objTransform.parent = parent;
                for (var x = 0; x < config.width; x++)
                {
                    for (var y = 0; y < config.height; y++)
                    {
                        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.localPosition =
                            new Vector3(x - config.width / 2f, 0, 1+y - config.height / 2f) * 2;
                        cube.transform.parent = objTransform;
                        cube.transform.localScale = Vector3.one * 1.8f;
                    }
                }
            }
            
            obj.transform.localPosition = Vector3.zero;

            return obj;
        }
    }
}