using System;
using Model;
using UnityEngine;
using Utils.Injection;

namespace View.UI.Dialogue
{
    public class DialogueTrigger : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interactionState;
        [Inject] private DialogInteractionStateModel _dialogInteraction;

        [SerializeField] private DialogueUI ui;
        private ChatNode _currentNode;

        private void Start()
        {
            _interactionState.Updated.Add(OnInteractionStateUpdated);
            _dialogInteraction.CurrentChatIndexUpdated.Add(OnChatNodeUpdated);
            OnInteractionStateUpdated();
        }

        private void OnInteractionStateUpdated()
        {
            ui.gameObject.SetActive(_interactionState.State == InteractionState.Dialog);
            if (_interactionState.State != InteractionState.Dialog) return;

            ui.answerSelected.RemoveAllListeners();
            ui.answerSelected.AddListener(OnAnswerSelected);

            OnChatNodeUpdated();
        }

        private void OnChatNodeUpdated()
        {
            if (_interactionState.State != InteractionState.Dialog) return;
            
            var currentChatNode = _dialogInteraction.GetCurrentChatNode();
            if (currentChatNode == null)
                ExitDialogue();
            else
                ui.ShowChat(currentChatNode);
        }

        private void OnAnswerSelected(int index)
        {
            _dialogInteraction.SubmitAnswer(index);
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
            _dialogInteraction.CurrentChatIndexUpdated.Remove(OnChatNodeUpdated);
        }
    }
}