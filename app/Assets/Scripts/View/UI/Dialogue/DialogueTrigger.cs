using System;
using Model;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace View.UI.Dialogue
{
    public class DialogueTrigger : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interactionState;
        [Inject] private DialogInteractionStateModel _dialogInteraction;

        [SerializeField] private DialogueUI ui;

        private void Start()
        {
            _interactionState.Updated.Add(OnInteractionStateUpdated);
            _dialogInteraction.ChatUpdated.Add(OnChatNodeUpdated);
            OnInteractionStateUpdated();
        }

        private void OnInteractionStateUpdated()
        {
            ui.gameObject.SetActive(_interactionState.State == InteractionState.Dialog);
            if (_interactionState.State != InteractionState.Dialog) return;

            ui.answerSelected.RemoveAllListeners();
            ui.answerSelected.AddListener(OnAnswerSelected);
            
            ui.gameObject.SetActive(false);
        }

        private void OnChatNodeUpdated()
        {
            if (_interactionState.State != InteractionState.Dialog) return;
            
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

            if (_interactionState.State == InteractionState.Dialog)
                _interactionState.SetState(InteractionState.Idle);
        }

        private void OnDestroy()
        {
            _interactionState.Updated.Remove(OnInteractionStateUpdated);
            _dialogInteraction.ChatUpdated.Remove(OnChatNodeUpdated);
        }
    }
}