using System;
using Model;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class DisplayCredits : InjectableBehaviour
    {
        private struct ResourceDiff
        {
            public int inc;
            public int dec;
        }

        [Inject] private SettlementModel _model;

        [SerializeField] private Text foodLabel;
        [SerializeField] private Text foodDiff;
        [SerializeField] private Image foodBar;

        [SerializeField] private Text woodLabel;
        [SerializeField] private Text woodDiff;
        [SerializeField] private Image woodBar;

        [SerializeField] private Text waterLabel;
        [SerializeField] private Text waterDiff;
        [SerializeField] private Image waterBar;

        [SerializeField] private Text stoneLabel;
        [SerializeField] private Text stoneDiff;
        [SerializeField] private Image stoneBar;

        [SerializeField] private Text coinsLabel;
        [SerializeField] private Text coinsDiff;
        [SerializeField] private Image coinsBar;

        private void Start()
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
                foodBar.fillAmount = GetProgress(settlement.Treasury.Food, caps.Food);
                foodDiff.text = FormatDiff(CalculateFoodDiff());

                woodLabel.text = FormatResource(settlement.Treasury.Wood, caps.Wood);
                woodBar.fillAmount = GetProgress(settlement.Treasury.Wood, caps.Wood);
                woodDiff.text = FormatDiff(CalculateWoodDiff());

                waterLabel.text = FormatResource(settlement.Treasury.Water, caps.Water);
                waterBar.fillAmount = GetProgress(settlement.Treasury.Water, caps.Water);
                waterDiff.text = FormatDiff(CalculateWaterDiff());

                stoneLabel.text = FormatResource(settlement.Treasury.Stone, caps.Stone);
                stoneBar.fillAmount = GetProgress(settlement.Treasury.Stone, caps.Stone);
                stoneDiff.text = FormatDiff(CalculateStoneDiff());

                coinsLabel.text = FormatResource(settlement.Treasury.Gold, caps.Gold);
                coinsBar.fillAmount = GetProgress(settlement.Treasury.Gold, caps.Gold);
                coinsDiff.text = FormatDiff(CalculateGoldDiff());
            }
        }

        private float GetProgress(ushort value, int max)
        {
            if (max < 1)
                return 1;

            return Mathf.Clamp((float)value / max, 0, 1);
        }

        private string FormatResource(int value, int cap)
        {
            return value < cap ? $"{value}/{cap}" : $"{value}/<color=red>{cap}</color>";
        }

        private static string FormatDiff(ResourceDiff diff)
        {
            var result = "";

            if (diff.inc > 0)
                result += $"<color=green>+{diff.inc}</color>";

            if (diff.dec > 0)
                result += $"<color=red>-{diff.dec}</color>";

            return result;
        }

        private ResourceDiff CalculateFoodDiff()
        {
            return new ResourceDiff
            {
                inc = _model.GetCollectionRate(BuildingType.FoodCollector),
                dec = _model.GetConsumptionRate()
            };
        }

        private ResourceDiff CalculateWoodDiff()
        {
            return new ResourceDiff
            {
                inc = _model.GetCollectionRate(BuildingType.WoodCollector),
                dec = 0
            };
        }

        private ResourceDiff CalculateWaterDiff()
        {
            return new ResourceDiff
            {
                inc = _model.GetCollectionRate(BuildingType.WaterCollector),
                dec = _model.GetConsumptionRate()
            };
        }

        private ResourceDiff CalculateStoneDiff()
        {
            return new ResourceDiff
            {
                inc = _model.GetCollectionRate(BuildingType.StoneCollector),
                dec = 0
            };
        }

        private ResourceDiff CalculateGoldDiff()
        {
            return new ResourceDiff
            {
                inc = _model.GetCollectionRate(BuildingType.GoldCollector),
                dec = 0
            };
        }

        private void OnDestroy()
        {
            _model.Updated.Remove(OnUpdated);
        }
    }
}