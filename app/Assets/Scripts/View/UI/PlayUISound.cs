
using System;
using Notifications;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.UI
{
    public class PlayUISound : InjectableBehaviour
    {
        [SerializeField]
        private AudioSource source;
        [Inject] private PlayAudioClip _play;

        private void Start()
        {
            
            DontDestroyOnLoad(gameObject);
            _play.Add(OnPlay);
            
        }

        private void OnPlay(AudioClip value)
        {
            source.PlayOneShot(value);
        }

        private void OnDestroy()
        {
            
            _play.Remove(OnPlay);
            
        }
    }
}
