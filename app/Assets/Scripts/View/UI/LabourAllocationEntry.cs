using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace View.UI
{
    public class LabourAllocationEntry : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject freeMarker;
        [SerializeField] private GameObject deadMarker;
        [SerializeField] private TMP_Text buildingIndex;

        private int _index;
        private int _allocation;

        [HideInInspector] public UnityEvent<int> onSelected;

        public void SetData(int index, int allocation)
        {
            _index = index;
            _allocation = allocation;

            if (buildingIndex)
                buildingIndex.text = _allocation >= 0 ? _allocation.ToString() : "x";

            if (freeMarker)
                freeMarker.SetActive(allocation == -1);
            
            if (deadMarker)
                deadMarker.SetActive(allocation < -1);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSelected?.Invoke(_index);
        }
    }
}