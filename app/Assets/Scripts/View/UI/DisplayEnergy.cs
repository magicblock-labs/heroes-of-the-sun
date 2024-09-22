using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class DisplayEnergy : InjectableBehaviour
    {
        [Inject] private SettlementModel _model;

        [SerializeField] private Text energyLabel;
        [SerializeField] private Graphic nextButton;
        private const int Cap = 10; //todo calc

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
                energyLabel.text = $"{settlement.TimeUnits}/{Cap}";

                nextButton.color = settlement.TimeUnits > 0
                    ? Color.green * 0.8f
                    : Color.red * 0.6f;
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}