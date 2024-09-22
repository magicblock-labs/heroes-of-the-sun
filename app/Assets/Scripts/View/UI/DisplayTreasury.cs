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
                foodLabel.text = $"{settlement.Treasury.Food}";
                woodLabel.text = $"{settlement.Treasury.Wood}";
                waterLabel.text = $"{settlement.Treasury.Water}";
                // stoneLabel.text = $"{settlement.Treasury.Water}";
                // coinsLabel.text = $"{settlement.Treasury.Water}";
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}
