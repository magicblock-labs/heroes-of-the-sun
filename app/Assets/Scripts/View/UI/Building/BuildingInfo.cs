using System.Collections.Generic;
using System.Linq;
using Model;
using Notifications;
using Plugins.Demigiant.DOTween.Modules;
using Service;
using TMPro;
using UnityEngine;
using Utils.Injection;
using View.ActionRequest;
using BuildingType = Settlement.Types.BuildingType;

namespace View.UI.Building
{
    public class BuildingInfo : BuildingUIPanel
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;
        [Inject] private InteractionStateModel _interaction;
        [Inject] private ShowWorkerSelection _showWorkerSelection;

        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text levelLabel;

        [SerializeField] private DeteriorationStatus deteriorationStatus;
        [SerializeField] private WorkerStatus workerStatus;
        [SerializeField] private ExtractionStatus extractionStatus;

        private IBuildingActionButton[] _actionButtons;
        [SerializeField] private GameObject controls;

        private readonly Dictionary<RectTransform, Vector2> _actionPositions = new();
        private int _index;

        protected override void Start()
        {
            base.Start();
            controls.SetActive(false);
            
            _actionButtons ??= GetComponentsInChildren<IBuildingActionButton>();

            foreach (RectTransform child in controls.transform)
                _actionPositions[child] = child.anchoredPosition;
        }

        public void SetData(int index, Settlement.Types.Building value)
        {
            if (value == null)
                return;

            _index = index;

            _actionButtons ??= GetComponentsInChildren<IBuildingActionButton>();

            foreach (var btn in _actionButtons)
                btn.SetData(index, value);

            nameLabel.text = value.Id.ToString();
            if (levelLabel)
                levelLabel.text = value.Level.ToString();

            //TODO max deterioration
            deteriorationStatus.gameObject.SetActive(value.Deterioration > 50);
            deteriorationStatus.SetStatus(value.Deterioration, 127);

            var needsWorkers = value.TurnsToBuild > 0 ||
                               value.Id is BuildingType.WoodCollector or BuildingType.FoodCollector
                                   or BuildingType.StoneCollector or BuildingType.GoldCollector;
            workerStatus.gameObject.SetActive(needsWorkers);
            if (needsWorkers)
                workerStatus.SetCount(_settlement.Get().WorkerAssignment.Count(w => w == _index));
            
            extractionStatus.gameObject.SetActive(value.Id is BuildingType.GoldCollector or BuildingType.StoneCollector);
            extractionStatus.SetCount(value.Extraction);
        }

        public void ShowExtendedControls(bool value)
        {
            if (controls.activeSelf == value)
                return;

            controls.SetActive(value);

            foreach (RectTransform child in controls.transform)
            {
                child.anchoredPosition = Vector2.zero;
                child.DOAnchorPos(_actionPositions[child], .1f);
            }
        }
    }
}