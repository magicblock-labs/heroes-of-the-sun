using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class DeteriorationStatus : MonoBehaviour
    {
        [SerializeField] private Image bar;
        [SerializeField] private TMP_Text percentageLabel;

        public void SetStatus(int value, int max)
        {
            var percentage = Mathf.Clamp(1 - (float)value / max, 0, 1);
            bar.fillAmount = percentage;
            percentageLabel.text = $"{percentage:P0}";

            var r = (byte)(byte.MaxValue * Math.Clamp((1 - percentage) * 2f, 0, 1f));
            var g = (byte)(byte.MaxValue * Math.Clamp(percentage * 2, 0f, 1f));

            bar.color = new Color32(r, g, 0, byte.MaxValue);
        }
    }
}