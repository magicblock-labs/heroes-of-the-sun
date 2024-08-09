using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View
{
    [RequireComponent(typeof(Text))]
    public class DisplayCredits : InjectableBehaviour
    {
        [Inject] private BalanceModel _model;
        private Text _text;

        void Start()
        {
            _text = GetComponent<Text>();
            _model.Updated.Add(OnUpdated);
            OnUpdated();
        }

        private void OnUpdated()
        {
            _text.text = _model.Get().ToString();
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}
