using Model;
using UnityEngine;
using Utils.Injection;

namespace View
{
    public class LookAtDialogue : InjectableBehaviour
    {
        [Inject] private InteractionStateModel _interaction;

        public GameObject left;
        public GameObject right;

        public float cameraSize = 3f;

        void Update()
        {
            if (_interaction.State != InteractionState.Dialog)
                return;

            var center = (left.transform.position + right.transform.position) / 2f;

            var diff = left.transform.position - right.transform.position;

            var targetPoint = center + Vector3.Cross(diff, Vector3.up) * 3 + Vector3.up * 2f;
            transform.position += (targetPoint - transform.position) * (UnityEngine.Time.deltaTime * 4);
            transform.LookAt(center);

            GetComponent<Camera>().orthographicSize +=
                (cameraSize - GetComponent<Camera>().orthographicSize) * UnityEngine.Time.deltaTime * 4;
        }
    }
}