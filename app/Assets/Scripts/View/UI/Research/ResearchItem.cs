using System;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI.Research
{
    public class ResearchItem : InjectableBehaviour, IPointerClickHandler
    {
        [Inject] private SettlementModel _settlement;

        private Action<SettlementModel.ResearchType> _callback;
        private SettlementModel.ResearchType _type;

        [SerializeField] private Text nameLabel;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject[] researchLevelMarkers;

        private void Start()
        {
            _settlement.Updated.Add(Redraw);
        }

        private void Redraw()
        {
            nameLabel.text = _type.ToString();
            icon.sprite = Resources.Load<Sprite>(_type.ToString());

            var currentResearchLevel = _settlement.GetResearchLevel(_type);
            for (var i = 0; i < researchLevelMarkers.Length; i++)
                researchLevelMarkers[i].SetActive(i < currentResearchLevel);
        }

        public void SetData(SettlementModel.ResearchType value, Action<SettlementModel.ResearchType> callback)
        {
            _type = value;
            _callback = callback;

            Redraw();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _callback?.Invoke(_type);
        }

        private void OnDestroy()
        {
            _settlement.Updated.Remove(Redraw);
        }
    }
}