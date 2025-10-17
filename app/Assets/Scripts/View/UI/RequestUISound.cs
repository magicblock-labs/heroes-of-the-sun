// Editor-only using statements
#if UNITY_EDITOR
using UnityEditor;
#endif
using Notifications;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class RequestUISound : InjectableBehaviour
    {
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip uiFlipSound;

        [Inject] private PlayAudioClip _play;

        protected override void Awake()
        {

            base.Awake();
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => Play(clickSound));
            }

            var toggle = GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(_ => Play(uiFlipSound));
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                _play.Dispatch(clip);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Add UIPlaySound to all Buttons & Toggles")]
        private static void AddPlayUISoundToAllUIElements()
        {
            var buttons = UnityEngine.Object.FindObjectsOfType<Button>(true);
            var toggles = UnityEngine.Object.FindObjectsOfType<Toggle>(true);

            foreach (var button in buttons)
            {
                if (button.GetComponent<RequestUISound>() == null)
                {
                    button.gameObject.AddComponent<RequestUISound>();
                    EditorUtility.SetDirty(button.gameObject);
                }
            }
            foreach (var toggle in toggles)
            {
                if (toggle.GetComponent<RequestUISound>() == null)
                {
                    toggle.gameObject.AddComponent<RequestUISound>();
                    EditorUtility.SetDirty(toggle.gameObject);
                }
            }
        }
#endif
    }
}
