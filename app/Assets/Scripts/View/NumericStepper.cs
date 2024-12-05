using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class NumericStepper : MonoBehaviour
    {
        [SerializeField] private int minValue = 0;
        [SerializeField] private int maxValue = 5; //todo config

        [SerializeField] private Text label;

        public void Increment()
        {
            var val = int.Parse(label.text);
            if (val < maxValue)
                label.text = $"{++val}";
        }

        public void Decrement()
        {
            var val = int.Parse(label.text);
            if (val > minValue)
                label.text = $"{--val}";
        }
    }
}