using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace View
{
    [RequireComponent(typeof(Text))]
    [RequireComponent(typeof(Button))]
    public class DialogueAnswer : MonoBehaviour
    {
        public UnityEvent<int> selected;
        private int _index;

        public void Setup(int index, string value)
        {
            _index = index;
            GetComponent<Text>().text = $"{index + 1}. {value}";
            GetComponent<Button>().onClick.AddListener(OnSelected);
        }

        private void OnSelected()
        {
            selected.Invoke(_index);
        }

        private void Update()
        {
            //ugh
            if (Input.GetKeyDown(KeyCode.Alpha1) && _index == 0)
                OnSelected();
            else if (Input.GetKeyDown(KeyCode.Alpha2) && _index == 1)
                OnSelected();
            else if (Input.GetKeyDown(KeyCode.Alpha3) && _index == 2)
                OnSelected();
        }
    }
}