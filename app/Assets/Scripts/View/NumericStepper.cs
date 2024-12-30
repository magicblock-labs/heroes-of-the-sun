using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace View
{
    public class NumericStepper : MonoBehaviour
    {
        [SerializeField] private int minValue = 0;
        [SerializeField] private int maxValue = 5; //todo config

        [SerializeField] private Text label;
        
        [SerializeField] private UnityEvent OnChanged;

        public void Increment()
        {
            var val = int.Parse(label.text);
            if (val < maxValue)
            {
                label.text = $"{++val}";
            }
        }

        public void Decrement()
        {
            var val = int.Parse(label.text);
            if (val > minValue)
            {
                label.text = $"{--val}";
                OnChanged?.Invoke();
            }
        }
    }
}