using Model;
using UnityEngine;
using Utils.Injection;

namespace View.RpgMode
{
    public class Follow : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interaction;

        public Transform target;
        public Vector3 offset;

        public float cameraSize = 8f;

        void Update()
        {
            if (_interaction.State == InteractionState.Dialog)
                return;

            var targetPoint = target.position + offset;
            transform.position += (targetPoint - transform.position) * UnityEngine.Time.deltaTime;
            transform.LookAt(target.position, Vector3.up);
            GetComponent<Camera>().orthographicSize +=
                (cameraSize - GetComponent<Camera>().orthographicSize) * UnityEngine.Time.deltaTime * 4;
            ;
        }
    }
}