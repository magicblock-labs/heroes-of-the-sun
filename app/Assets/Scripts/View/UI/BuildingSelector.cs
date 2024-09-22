using System;
using Model;
using Service;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class BuildingSelector : InjectableBehaviour
    {
        [Inject] private ConfigModel _config;
        [Inject] private SettlementModel _model;
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private RawImage snapshot;
        [SerializeField] private Camera cam;
        [SerializeField] private Transform buildingContainer;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text costLabel;

        private Action<BuildingType> _selectedCallback;
        private BuildingType _type;

        public void SetData(BuildingType value)
        {
            _type = value;

            var renderTex = new RenderTexture(512, 512, 16);
            snapshot.texture = renderTex;
            cam.targetTexture = renderTex;
            var buildingConfig = _config.Buildings[value];
            BuildingPreview.CreateBuildingInto(buildingConfig, buildingContainer);

            cam.Render();
            cam.enabled = false;
            Destroy(buildingContainer.gameObject);

            nameLabel.text = value.ToString();
            costLabel.text = buildingConfig.cost.ToString();
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