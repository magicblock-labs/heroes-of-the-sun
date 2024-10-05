using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Building
{
    public class ExtractionStatus : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text countLabel;

        public void SetCount(int value)
        {
            icon.color = value > 0 ? Color.white : Color.red;
            countLabel.text = $"{value}";
        }
    }
}