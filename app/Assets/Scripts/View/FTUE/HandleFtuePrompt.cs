using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Model;
using Notifications;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.Ftue
{
    public class HandleFtuePrompt : InjectableBehaviour
    {
        [Inject] ShowFtuePrompt _showFtuePrompt;
        [Inject] HideFtuePrompt _hideFtuePrompt;
        
        [Inject] StopFtueSequence _stopFtueSequence;
        [Inject] GridInteractionStateModel _gridInteraction;

        [SerializeField] private RectTransform uiRoot;

        [Header("prompt content")] [SerializeField]
        private RectTransform promptContainer;

        [SerializeField] private Text promptText;

        [Header("blocker")] [SerializeField] private GameObject blockerContainer;
        [SerializeField] private RectTransform cutout;

        // private static HandleFtuePrompt _instance;
        private FtuePrompt[] _prompts;
        private int _index;
        private const float Padding = 10;
        private const float Gap = 30;

        private void Start()
        {
            _showFtuePrompt.Add(OnShowFtuePrompts);
            _hideFtuePrompt.Add(OnHideFtuePrompt);

            OnHideFtuePrompt();
        }

        private void OnShowFtuePrompts(FtuePrompt[] value)
        {
            _index = 0;
            _prompts = value;

            ShowCurrentPrompt();
        }

        private void ShowCurrentPrompt()
        {
            var value = _prompts[_index];

            blockerContainer.gameObject.SetActive(true);
            var canvasRect = TransformScreenRectToCanvas(value.cutoutScreenSpace);
            cutout.anchoredPosition = canvasRect.position;
            cutout.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasRect.size.x);
            cutout.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasRect.size.y);

            
            promptText.text = value.promptText;

            var settings = promptText.GetGenerationSettings(promptText.rectTransform.rect.size);
            
            var textGen = new TextGenerator();
            promptContainer.sizeDelta = new Vector2(
                textGen.GetPreferredWidth(value.promptText, settings) * .7f + Padding,
                textGen.GetPreferredHeight(value.promptText, settings) + Padding);
            
            var offsetRect = promptContainer.rect.size + canvasRect.size + Vector2.one * Gap;
            promptContainer.anchoredPosition = cutout.anchoredPosition+offsetRect / 2 * value.promptLocation;
            promptContainer.gameObject.SetActive(true);

            _gridInteraction.LockOverride = true;
        }

        private Rect TransformScreenRectToCanvas(Rect value)
        {
            return new Rect(value.x * ScaleFactor, value.y * ScaleFactor,
                value.width * ScaleFactor, value.height * ScaleFactor);
        }

        private float ScaleFactor => 1 / uiRoot.lossyScale.x;

        private void Update()
        {
            if (_prompts != null && _prompts.Length > _index && Input.GetMouseButtonDown(0) &&
                !_prompts[_index].blocking)
            {
                Hide();
            }
        }

        private void OnHideFtuePrompt()
        {
            promptContainer.gameObject.SetActive(false);
            blockerContainer.gameObject.SetActive(false);
            _prompts = null;
            _gridInteraction.LockOverride = false;
        }

        public void Hide()
        {
            _index++;
            if (_index < _prompts.Length)
                ShowCurrentPrompt();
            else
                _hideFtuePrompt.Dispatch();
        }

        public void StopFtueSequence()
        {
            _stopFtueSequence.Dispatch();
        }

        private void OnDestroy()
        {
            _showFtuePrompt.Remove(OnShowFtuePrompts);
            _hideFtuePrompt.Remove(OnHideFtuePrompt);
        }
    }
}