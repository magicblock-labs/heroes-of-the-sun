using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI.Building
{
    public class BuildingSnapshot : InjectableBehaviour
    {
        [SerializeField] private RawImage snapshot;
        [SerializeField] private Camera cam;
        [SerializeField] private Transform buildingContainer;

        public void Generate(BuildingConfig buildingConfig)
        {
            var renderTex = new RenderTexture(512, 512, 16);
            snapshot.texture = renderTex;
            cam.targetTexture = renderTex;
            BuildingPreview.CreateBuildingInto(buildingConfig, buildingContainer);

            cam.Render();
            cam.enabled = false;
            Destroy(buildingContainer.gameObject);
        }

        private void OnDestroy()
        {
            if (snapshot.texture)
                Destroy(snapshot.texture); //destroy generated texture
        }
    }
}