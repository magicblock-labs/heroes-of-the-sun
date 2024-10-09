using System;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI.Building
{
    public class BuildingProgress : BuildingUIPanel
    {
        [Inject] private SettlementModel _settlement;
        
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private Image bg;
        [SerializeField] private Image fill;
        [SerializeField] private TMP_Text statePercentageLabel;

        public void SetData(Settlement.Types.Building value, BuildingConfig config)
        {
            if (value == null)
                return;

            if (value.TurnsToBuild <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            nameLabel.text = value.Id.ToString();

            var totalBuildTime = _settlement.GetBuildTime(config.buildTimeTier, value.Level) ;
            var percentage =
                totalBuildTime > 0
                    ? Mathf.Clamp((float)(totalBuildTime - value.TurnsToBuild) / totalBuildTime, 0, 1)
                    : 1;

            fill.fillAmount = percentage;
            statePercentageLabel.text = $"{percentage:0%}";

            var r = (byte)(byte.MaxValue * Math.Clamp((1 - percentage) * 2f, 0, 1f));
            var g = (byte)(byte.MaxValue * Math.Clamp(percentage * 2, 0f, 1f));

            bg.color = new Color32(r, g, 0, 40);
            fill.color = new Color32(r, g, 0, byte.MaxValue);
        }
    }
}