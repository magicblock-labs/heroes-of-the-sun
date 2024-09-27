using Model;
using Service;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View.UI
{
    public class LabourAllocationEntry : InjectableBehaviour, IPointerClickHandler
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ConfigModel _config;
        
        [SerializeField] private GameObject freeMarker;
        [SerializeField] private GameObject deadMarker;
        [SerializeField] private BuildingSnapshot buildingSnapshot;

        private int _index;
        private int _allocation;

        [HideInInspector] public UnityEvent<int> onSelected;

        public void SetData(int index, int allocation)
        {
            _index = index;
            _allocation = allocation;

            Invoke(nameof(CreateBuildingSnapshot), .01f);

            if (freeMarker)
                freeMarker.SetActive(allocation == -1);
            
            if (deadMarker)
                deadMarker.SetActive(allocation < -1);
        }

        void CreateBuildingSnapshot()
        {
            if (!buildingSnapshot) return;
            
            var building = _settlement.Get().Buildings[_allocation];
            var buildingConfig = _config.Buildings[(BuildingType)building.Id];
                
            buildingSnapshot.Generate(buildingConfig);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSelected?.Invoke(_index);
        }
    }
}