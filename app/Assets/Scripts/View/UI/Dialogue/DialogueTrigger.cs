using Model;
using UnityEngine;
using Utils.Injection;

namespace View.UI.Dialogue
{
    public class DialogueTrigger : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interactionState;

        [SerializeField] private DialogueUI ui;
        private ChatNode _currentNode;

        private void Start()
        {
            _interactionState.Updated.Add(OnUpdated);
            OnUpdated();
        }

        private void OnUpdated()
        {
            ui.gameObject.SetActive(_interactionState.State == InteractionState.Dialog);
            if (_interactionState.State != InteractionState.Dialog) return;

            ui.answerSelected.RemoveAllListeners();
            ui.answerSelected.AddListener(OnAnswerSelected);

            _currentNode = new ChatNode()
            {
                prompt = "Would you like to interact with this totem?",
                answers = new Answer[]
                {
                    new()
                    {
                        text = "Hm, shake it a bit?",
                        nextNode = new ChatNode()
                        {
                            prompt = "Nothing happens...",
                            answers = new Answer[]
                            {
                                new()
                                {
                                    text = "Walk away.",
                                    nextNode = null
                                }
                            }
                        }
                    },
                    new()
                    {
                        text = "Yes please!",
                        nextNode = new ChatNode()
                        {
                            prompt = "Ok, give me one unit of food!",
                            answers = new Answer[]
                            {
                                new()
                                {
                                    text = "Sure, here you go!",
                                    nextNode = null
                                },
                                new()
                                {
                                    text = "Ignore and walk away",
                                    nextNode = null
                                }
                            }
                        }
                    },
                    new()
                    {
                        text = "Nevermind",
                        nextNode = null
                    }
                }
            };
            ui.ShowChat(_currentNode);
        }

        private void OnAnswerSelected(int index)
        {
            if (_currentNode.answers[index].nextNode == null)
            {
                ExitDialogue();
                return;
            }

            _currentNode = _currentNode.answers[index].nextNode;
            ui.ShowChat(_currentNode);
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
            _interactionState.Updated.Remove(OnUpdated);
        }
    }
}