using Connectors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace View.UI.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        public Text prompt;
        public Transform answers;
        public DisplayDialogueAnswer answerPrefab;

        [HideInInspector] public UnityEvent<int> answerSelected;

        public void ShowChat(ChatNode chatNode)
        {
            prompt.text = chatNode?.reply;

            foreach (Transform child in answers)
            {
                child.GetComponent<DisplayDialogueAnswer>().selected.RemoveAllListeners();
                Destroy(child.gameObject);
            }

            if (chatNode == null) return;

            if (chatNode.options?.Length > 0)
            {
                var i = 0;
                foreach (var answer in chatNode.options)
                {
                    var dialogueAnswer = Instantiate(answerPrefab, answers);
                    dialogueAnswer.Setup(++i, answer);
                    dialogueAnswer.selected.AddListener(OnAnswerSelected);
                }
            }
            else
            {
                var dialogueAnswer = Instantiate(answerPrefab, answers);
                dialogueAnswer.Setup(1, "Leave..");
                dialogueAnswer.selected.AddListener(OnAnswerSelected);
            }
        }

        private void OnAnswerSelected(int index)
        {
            answerSelected.Invoke(index);
        }
    }
}