using System;
using Model;
using Service;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;
using Vector3 = System.Numerics.Vector3;

namespace View
{
    public class BuildingSelector : InjectableBehaviour
    {
        [Inject] private ConfigModel _config;
        [Inject] private BuildingsModel _model;
        [Inject] private InteractionStateModel _interaction;
        
        [SerializeField] private RawImage snapshot;
        [SerializeField] private Camera cam;
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private Text id;

        private Action<BuildingType> _selectedCallback;
        private BuildingType _type;

        public void SetData(BuildingType value)
        {
            _type = value;
            
            var renderTex = new RenderTexture(256, 256, 16);
            snapshot.texture = renderTex;
            cam.targetTexture = renderTex;
            var buildingConfig = _config.Buildings[value];
            cam.orthographicSize = (float)Math.Sqrt(buildingConfig.width) * 3;
            
            BuildingPreview.GenerateBuilding(buildingConfig, buildingContainer);
            buildingContainer.localPosition = UnityEngine.Vector3.zero;

            cam.Render();
            cam.enabled = false;
            Destroy(buildingContainer.gameObject);

            id.text = value.ToString();
        }

        public void OnClick()
        {
            _interaction.StartPlacement(_type);
        }

        private void OnDestroy()
        {
            if (snapshot.texture)
                Destroy(snapshot.texture); //destroy generated texture
        }
    }
}