using System;
using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class DisplayFaith : InjectableBehaviour
    {
        [Inject] private SettlementModel _model;

        [SerializeField] private Image progressBar;
        private const int Cap = 127; //TODO calc

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
                var percentage = Mathf.Clamp((float)settlement.Faith / Cap, 0, 1);

                progressBar.fillAmount = percentage;
                var r = (byte)(byte.MaxValue * Math.Clamp((1 - percentage) * 2f, 0, 1f));
                var g = (byte)(byte.MaxValue * Math.Clamp(percentage * 2, 0f, 1f));
                progressBar.color = new Color32(r, g, 0, byte.MaxValue);
            }
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}