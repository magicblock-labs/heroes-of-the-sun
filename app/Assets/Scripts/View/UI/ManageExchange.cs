using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Connectors;
using Model;
using Solana.Unity.Wallet;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class ManageExchange : InjectableBehaviour
    {
        [Inject] private PlayerSettlementConnector _settlement;

        [SerializeField] private Text resourceFood;
        [SerializeField] private Text resourceWood;
        [SerializeField] private Text resourceWater;
        [SerializeField] private Text resourceStone;

        [SerializeField] private Text multiplierFood;
        [SerializeField] private Text multiplierWood;
        [SerializeField] private Text multiplierWater;
        [SerializeField] private Text multiplierStone;

        [SerializeField] private Text costLabel;

        private void OnEnable()
        {
            resourceFood.text = "0";
            resourceWood.text = "0";
            resourceWater.text = "0";
            resourceStone.text = "0";

            var rates = SettlementModel.GetExchangeRates();
            multiplierFood.text = $"x{rates.Food}";
            multiplierWood.text = $"x{rates.Wood}";
            multiplierWater.text = $"x{rates.Water}";
            multiplierStone.text = $"x{rates.Stone}";
        }

        public void UpdateCost()
        {
            var cost = int.Parse(resourceFood.text) +
                       int.Parse(resourceWater.text) +
                       int.Parse(resourceWood.text) +
                       int.Parse(resourceStone.text);
            costLabel.text = $"{cost}";
        }

        public void OnSubmit()
        {
            _ = ExchangeAsync();
        }

        private async Task ExchangeAsync()
        {
            Dimmer.Visible = true;

            await _settlement.Exchange(
                int.Parse(resourceFood.text),
                int.Parse(resourceWater.text),
                int.Parse(resourceWood.text),
                int.Parse(resourceStone.text)
            );

            Dimmer.Visible = false;
        }
    }
}