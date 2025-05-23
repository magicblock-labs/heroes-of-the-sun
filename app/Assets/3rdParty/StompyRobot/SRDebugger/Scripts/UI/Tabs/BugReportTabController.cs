﻿using StompyRobot.SRDebugger.Scripts.Services;
using StompyRobot.SRDebugger.Scripts.UI.Other;
using StompyRobot.SRF.Scripts.Components;
using StompyRobot.SRF.Scripts.Helpers;
using StompyRobot.SRF.Scripts.Service;
using UnityEngine;

namespace StompyRobot.SRDebugger.Scripts.UI.Tabs
{
    public class BugReportTabController : SRMonoBehaviourEx, IEnableTab
    {
        [RequiredField] public BugReportSheetController BugReportSheetPrefab;

        [RequiredField] public RectTransform Container; 

        public bool IsEnabled
        {
            get { return SRServiceManager.GetService<IBugReportService>().IsUsable; }
        }
        
        protected override void Start()
        {
            base.Start();

            var sheet = SRInstantiate.Instantiate(BugReportSheetPrefab);
            sheet.IsCancelButtonEnabled = false;

            // Callbacks when taking screenshot will hide the debug panel so it is not present in the image
            sheet.TakingScreenshot = TakingScreenshot;
            sheet.ScreenshotComplete = ScreenshotComplete;

            sheet.CachedTransform.SetParent(Container, false);
        }

        private void TakingScreenshot()
        {
            SRDebug.Instance.HideDebugPanel();
        }

        private void ScreenshotComplete()
        {
            SRDebug.Instance.ShowDebugPanel(false);
        }
    }
}
