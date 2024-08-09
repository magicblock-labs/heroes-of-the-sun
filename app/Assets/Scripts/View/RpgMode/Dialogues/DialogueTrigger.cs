using Model;
using QuickOutline.Scripts;
using Tools.DataAssets.Nodes;
using UnityEngine;
using UnityEngine.AI;
using Utils.Injection;

namespace View
{
    [RequireComponent(typeof(Collider))]
    public class DialogueTrigger : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interaction;

        [SerializeField] private GameObject indicator;

        private DialogueUI _ui;
        private static readonly int Talk = Animator.StringToHash("Talk");
        [SerializeField] private DialogueGraph graph;
        public byte Cooldown;
        public byte Id;

        public static byte CurrentDialogue;
        private bool _canTalk = true;

        public void UpdateCurrentQuest()
        {
            if (Cooldown > 0)
                graph = Resources.Load<DialogueGraph>("Busy");

            _canTalk = Cooldown == 0;
            
            var parent = transform.parent;

            var outline = parent.GetComponent<Outline>();

            if (outline != null)
                outline.enabled = graph != null;
            if (indicator != null)
                indicator.SetActive(graph != null && _canTalk);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_canTalk)
                return;
                
            var nav = other.GetComponent<NavMeshAgent>();
            if (nav != null)
                nav.ResetPath();

            other.transform.LookAt(transform.parent);

            if (transform.parent.GetComponent<Animator>() != null)
                transform.parent.LookAt(other.transform);

            _ui = FindObjectOfType<DialogueUI>();

            _ui.answerSelected.RemoveAllListeners();
            _ui.answerSelected.AddListener(OnAnswerSelected);

            if (graph == null)
                return;

            graph.Start();
            _ui.Setup(transform.parent);
            _ui.ShowChat(graph.current);
            StartDialogue();
        }

        private void StartDialogue()
        {
            CurrentDialogue = Id;
            
            if (transform.parent.GetComponent<Animator>() != null)
                transform.parent.GetComponent<Animator>().SetTrigger(Talk);

            _ui.transform.GetChild(0).gameObject.SetActive(true);

            Camera.main.GetComponent<LookAtDialogue>().right = gameObject;

            _interaction.SetState(InteractionState.Dialog);
        }

        private void OnAnswerSelected(int index)
        {
            graph.SubmitAnswer(index);

            if (graph.current == null)
                ExitDialogue();
            else
            {
                _ui.ShowChat(graph.current);

                if (transform.parent.GetComponent<Animator>() != null)
                    transform.parent.GetComponent<Animator>().SetTrigger(Talk);
            }
        }

        private void ExitDialogue()
        {
            if (_ui != null)
            {
                _ui.answerSelected.RemoveAllListeners();
                _ui.transform.GetChild(0).gameObject.SetActive(false);
            }

            if (_interaction.State == InteractionState.Dialog)
                _interaction.SetState(InteractionState.Idle);
        }
    }
}