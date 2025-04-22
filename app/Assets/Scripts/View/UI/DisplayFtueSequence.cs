using System.Collections;
using Model;
using Notifications;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class DisplayFtueSequence : InjectableBehaviour
    {
        [Inject] StartFtueSequence _startFtueSequence;
        [Inject] StopFtueSequence _stopFtueSequence;
    
        [Inject] ShowFtuePrompt _showFtue;
        [Inject] HideFtuePrompt _hideFtue;
    
        [Inject] NavigationContextModel _nav;

        private void Start()
        {
            _startFtueSequence.Add(OnStartFtue);
            _stopFtueSequence.Add(OnStopFtue);
        }

        private void OnStartFtue(QuestData questData)
        {
            StartCoroutine(DisplayTutorial(questData));
        }

        private void OnStopFtue()
        {
            _hideFtue.Dispatch();
            StopAllCoroutines();
        }

        private IEnumerator DisplayTutorial(QuestData questData)
        {
            _showFtue.Dispatch(new []
            {
                new FtuePrompt
                {
                    blocking = true, promptLocation = Vector2Int.right, promptText = "Open Build Menu", cutoutScreenSpace = new Rect(200, 120, 160, 160)
                }
            });

            yield return new WaitUntil(() => _nav.IsBuildMenuOpen);
            _hideFtue.Dispatch();
        }

    
        private void OnDestroy()
        {
            _startFtueSequence.Remove(OnStartFtue);
            _stopFtueSequence.Remove(OnStopFtue);
        }
    }
}
