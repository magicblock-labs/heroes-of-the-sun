using System;
using System.Collections;
using Service;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class BuildingProgress : BuildingUIPanel
    {
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text levelLabel;
        [SerializeField] private Image bg;
        [SerializeField] private Image fill;
        [SerializeField] private Text statePercentageLabel;

        public void SetData(Building value, BuildingConfig config)
        {
            if (value == null)
                return;

            if (value.TurnsToBuild <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            nameLabel.text = value.Id.ToString();
            if (levelLabel)
                levelLabel.text = "Level: " + value.Level;

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