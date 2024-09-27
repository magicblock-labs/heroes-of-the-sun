using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class DisplayCredits : InjectableBehaviour
    {
        [Inject] private SettlementModel _model;

        [SerializeField] private Text foodLabel;
        [SerializeField] private Text woodLabel;
        [SerializeField] private Text waterLabel;
        [SerializeField] private Text stoneLabel;
        [SerializeField] private Text coinsLabel;

        void Start()
        {
            _model.Updated.Add(OnUpdated);
            OnUpdated();
        }

        private void OnUpdated()
        {
            if (_model.HasData)
            {
                var settlement = _model.Get();
                var caps = _model.StorageCapacity;
                foodLabel.text = FormatResource(settlement.Treasury.Food, caps.Food);
                woodLabel.text = FormatResource(settlement.Treasury.Wood, caps.Wood);
                waterLabel.text = FormatResource(settlement.Treasury.Water, caps.Water);
                // stoneLabel.text = $"{settlement.Treasury.Water}";
                // coinsLabel.text = $"{settlement.Treasury.Water}";
            }
        }

        string FormatResource(int value, int cap)
        {
            return value < cap ? $"{value}/{cap}" : $"{value}/<color=red>{cap}</color>";
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}