using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Building
{
    public class WorkerStatus : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text countLabel;

        public void SetCount(int value)
        {
            icon.color = value > 0 ? Color.white : Color.red;
            countLabel.text = $"x{value} {(value == 0 ? "!" : "")}";
        }
    }
}