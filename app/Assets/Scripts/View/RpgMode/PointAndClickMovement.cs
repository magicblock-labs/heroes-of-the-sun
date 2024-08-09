using Model;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Utils.Injection;

namespace View
{
    public class PointAndClickMovement : InjectableBehaviour
    {
    
        [Inject] private InteractionStateModel _interaction;
    
        public NavMeshAgent agent;

        private Vector3 _previousPos;
        private static readonly int Speed = Animator.StringToHash("Speed");

        // Update is called once per frame
        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
        
            if (_interaction.State == InteractionState.Idle && Input.GetMouseButtonDown(0))
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(mouseRay, out RaycastHit info, 100f))
                {
                    agent.SetDestination(info.point);
                }
            }
        }
    }
}