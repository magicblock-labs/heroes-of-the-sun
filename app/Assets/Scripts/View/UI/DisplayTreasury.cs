using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View
{
    [RequireComponent(typeof(Text))]
    public class DisplayCredits : InjectableBehaviour
    {
        [Inject] private SettlementModel _model;
        private Text _text;

        void Start()
        {
            _text = GetComponent<Text>();
            _model.Updated.Add(OnUpdated);
            OnUpdated();
        }

        private void OnUpdated()
        {
            if (_model.HasData)
            {
                var settlement = _model.Get();
                _text.text = $"Water: {settlement.Treasury.Water}; Food: {settlement.Treasury.Food}; Wood: {settlement.Treasury.Wood} DAY[{settlement.Day}]";
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}
