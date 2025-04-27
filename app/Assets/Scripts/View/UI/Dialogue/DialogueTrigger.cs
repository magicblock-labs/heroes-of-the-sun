using System;
using Model;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace View.UI.Dialogue
{
    public class DialogueTrigger : InjectableBehaviour
    {
        [Inject] private GridInteractionStateModel _gridInteractionState;
        [Inject] private DialogInteractionStateModel _dialogInteraction;

        [SerializeField] private DialogueUI ui;

        private void Start()
        {
            _gridInteractionState.Updated.Add(OnInteractionStateUpdated);
            _dialogInteraction.ChatUpdated.Add(OnChatNodeUpdated);
            OnInteractionStateUpdated();
        }

        private void OnInteractionStateUpdated()
        {
            ui.gameObject.SetActive(_gridInteractionState.State == GridInteractionState.Dialog);
            if (_gridInteractionState.State != GridInteractionState.Dialog) return;

            ui.answerSelected.RemoveAllListeners();
            ui.answerSelected.AddListener(OnAnswerSelected);
            
            ui.gameObject.SetActive(false);
        }

        private void OnChatNodeUpdated()
        {
            if (_gridInteractionState.State != GridInteractionState.Dialog) return;
            
            var currentChatNode = _dialogInteraction.GetCurrentChat();
            if (currentChatNode == null)
                ExitDialogue();
            else
            {
                ui.gameObject.SetActive(true);
                ui.ShowChat(currentChatNode);
            }
        }

        private void OnAnswerSelected(int index)
        {
            if (_dialogInteraction.GetCurrentChat().options?.Length > 1)
                _dialogInteraction.SubmitAnswer(index);
            else 
                ExitDialogue();
        }

        private void ExitDialogue()
        {
            if (ui != null)
                ui.answerSelected.RemoveAllListeners();

            if (_gridInteractionState.State == GridInteractionState.Dialog)
                _gridInteractionState.SetState(GridInteractionState.Idle);
        }

        private void OnDestroy()
        {
            _gridInteractionState.Updated.Remove(OnInteractionStateUpdated);
            _dialogInteraction.ChatUpdated.Remove(OnChatNodeUpdated);
        }
    }
}