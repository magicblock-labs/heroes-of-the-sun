using System;
using System.Collections.Generic;
using System.Linq;
using Connectors;
using Model;
using Notifications;
using DG.Tweening;
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
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private ShowWorkerSelection _showWorkerSelection;
        [Inject] private CtaRegister _ctaRegister;

        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text levelLabel;

        [SerializeField] private DeteriorationStatus deteriorationStatus;
        [SerializeField] private WorkerStatus workerStatus;
        [SerializeField] private ExtractionStatus extractionStatus;

        private IBuildingActionButton[] _actionButtons;
        [SerializeField] public GameObject controls;

        private readonly Dictionary<RectTransform, Vector2> _actionPositions = new();
        private int _index;
        private BuildingType? _type;

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

            _ctaRegister.Add(transform, CtaTag.PlacedBuilding, value.Id);

            _index = index;
            _type = value.Id;

            _actionButtons ??= GetComponentsInChildren<IBuildingActionButton>();

            foreach (var btn in _actionButtons)
                btn.SetData(index, value);

            nameLabel.text = value.Id.ToString();
            if (levelLabel)
                levelLabel.text = value.Level.ToString();

            var maxDeterioration = _settlement.GetMaxDeterioration();
            deteriorationStatus.gameObject.SetActive(value.Deterioration > maxDeterioration / 2);
            deteriorationStatus.SetStatus(value.Deterioration, (int)maxDeterioration);

            var needsWorkers = value.TurnsToBuild > 0 ||
                               value.Id is BuildingType.WoodCollector or BuildingType.FoodCollector
                                   or BuildingType.StoneCollector;
            workerStatus.gameObject.SetActive(needsWorkers);
            if (needsWorkers)
                workerStatus.SetCount(_settlement.Get().WorkerAssignment.Count(w => w == _index));

            extractionStatus.gameObject.SetActive(
                value.Id is BuildingType.StoneCollector
                && value.TurnsToBuild == 0);
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
                if (value && _actionPositions.TryGetValue(child, out var pos))
                    child.DOAnchorPos(pos, .1f);
            }
        }

        private void OnDestroy()
        {
            if (_type.HasValue)
                _ctaRegister.Remove(CtaTag.PlacedBuilding, _type.Value);
        }
    }
}