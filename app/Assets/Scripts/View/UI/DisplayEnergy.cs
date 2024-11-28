using System;
using System.Collections;
using Connectors;
using Model;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Injection;

namespace View.UI
{
    public class DisplayEnergy : InjectableBehaviour
    {
        [Inject] private SettlementModel _model;
        [Inject] private PlayerSettlementConnector _connector;

        [SerializeField] private Text timerLabel;
        [SerializeField] private Text energyLabel;
        [SerializeField] private Graphic nextButton;
        private long _claimTimestamp;

        private void Start()
        {
            _model.Updated.Add(OnUpdated);
            OnUpdated();
        }

        private void OnUpdated()
        {
            if (!_model.HasData) return;

            var settlement = _model.Get();
            energyLabel.text = $"{settlement.TimeUnits}/{_model.GetEnergyCap()}";

            nextButton.color = settlement.TimeUnits > 0
                ? Color.green * 0.8f
                : Color.red * 0.6f;


            _claimTimestamp = _model.GetNextEnergyClaimTimestamp();
            StopAllCoroutines();
            StartCoroutine(UpdateTimer());
        }

        private IEnumerator UpdateTimer()
        {
            while (true)
            {
                var secondsUntilClaim = _claimTimestamp - Web3Utils.GetNodeTime();
                if (secondsUntilClaim < 0)
                {
                    yield return new WaitForSeconds(1);
                    TryClaim();
                    StopAllCoroutines();
                    yield break;
                }

                timerLabel.text = FormatTimeRemaining(secondsUntilClaim);

                yield return new WaitForSeconds(1);
            }
        }

        private async void TryClaim()
        {
            await _connector.ClaimTime();
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }

        private static string FormatTimeRemaining(long remainingTime)

        {
            if (remainingTime <= 0)
                return "refreshing..";

            var timespanRemaining = TimeSpan.FromSeconds(remainingTime);

            if (timespanRemaining.Days > 1)
                return $"{timespanRemaining.Days}d{timespanRemaining.Hours}h";


            if (timespanRemaining.Hours > 1)
                return $"{timespanRemaining.Hours}h{timespanRemaining.Minutes}m";


            return $"{timespanRemaining.Minutes}m{timespanRemaining.Seconds}s";
        }
    }
}