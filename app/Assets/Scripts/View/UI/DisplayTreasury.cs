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
                _text.text = $"Water: {_model.Get().Treasury.Water}; Food: {_model.Get().Treasury.Food}; Wood: {_model.Get().Treasury.Wood}";
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}
