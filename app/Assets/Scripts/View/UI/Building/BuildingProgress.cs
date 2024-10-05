using System;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Building
{
    public class BuildingProgress : BuildingUIPanel
    {
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

            var percentage =
                config.buildTime > 0
                    ? Mathf.Clamp((float)(config.buildTime - value.TurnsToBuild) / config.buildTime, 0, 1)
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