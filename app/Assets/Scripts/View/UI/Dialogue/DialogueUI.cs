using System;
using Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
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
            prompt.text = chatNode.prompt;

            foreach (Transform child in answers)
            {
                child.GetComponent<DisplayDialogueAnswer>().selected.RemoveAllListeners();
                Destroy(child.gameObject);
            }

            var i = 0;
            foreach (var answer in chatNode.answers)
            {
                var dialogueAnswer = Instantiate(answerPrefab, answers);
                dialogueAnswer.Setup(i++, answer);
                dialogueAnswer.selected.AddListener(OnAnswerSelected);
            }
        }

        private void OnAnswerSelected(int index = -1)
        {
            answerSelected.Invoke(index);
        }
    }
}