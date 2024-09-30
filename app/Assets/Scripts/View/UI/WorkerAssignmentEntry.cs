using System.Collections.Generic;
using System.Linq;
using Model;
using Service;
using Settlement.Types;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class WorkerAssignmentEntry : InjectableBehaviour, IPointerClickHandler
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ConfigModel _config;
        
        [SerializeField] private GameObject freeMarker;
        [SerializeField] private GameObject deadMarker;
        [SerializeField] private BuildingSnapshot buildingSnapshot;
        [SerializeField] private Text workerCount;

        private sbyte _buildingIndex;
        private List<int> _workerIndexes;

        [HideInInspector] public UnityEvent<int> onSelected;

        public void SetData(sbyte building, List<int> workers)
        {
            _buildingIndex = building;
            _workerIndexes = workers;

            Invoke(nameof(CreateBuildingSnapshot), .01f);

            if (freeMarker)
                freeMarker.SetActive(_buildingIndex == -1);
            
            if (deadMarker)
                deadMarker.SetActive(_buildingIndex < -1);

            workerCount.text = $"x{_workerIndexes.Count}";
        }

        void CreateBuildingSnapshot()
        {
            if (!buildingSnapshot) return;
            
            var building = _settlement.Get().Buildings[_buildingIndex];
            var buildingConfig = _config.Buildings[building.Id];
                
            buildingSnapshot.Generate(buildingConfig);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSelected?.Invoke(_workerIndexes.Last());
        }
    }
}