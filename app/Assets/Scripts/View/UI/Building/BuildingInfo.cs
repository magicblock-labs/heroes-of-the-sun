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
    public class BuildingInfo : AnchoredUIPanel
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private ShowWorkerSelection _showWorkerSelection;
        [Inject] private CtaRegister _ctaRegister;
        [Inject] private ResourceDiffNotification _resourceDiff;
        [Inject] private NextTurnNotification _nextTurn;
        [Inject] private ConfigModel _config;

        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text levelLabel;

        [SerializeField] private DeteriorationStatus deteriorationStatus;
        [SerializeField] private WorkerStatus workerStatus;
        [SerializeField] private ExtractionStatus extractionStatus;

        private IBuildingActionButton[] _actionButtons;
        [SerializeField] public GameObject controls;

        private readonly Dictionary<RectTransform, Vector2> _actionPositions = new();
        private int _index;
        private Settlement.Types.Building _building;

        protected override void Start()
        {
            base.Start();
            controls.SetActive(false);

            _actionButtons ??= GetComponentsInChildren<IBuildingActionButton>();

            foreach (RectTransform child in controls.transform)
                _actionPositions[child] = child.anchoredPosition;

            _nextTurn.Add(OnNextTurn);
        }

        public void SetData(int index, Settlement.Types.Building value)
        {
            if (value == null)
                return;

            _building = value;

            _ctaRegister.Add(transform, CtaTag.PlacedBuilding, (int?)_building.Id);

            _index = index;

            _actionButtons ??= GetComponentsInChildren<IBuildingActionButton>();

            foreach (var btn in _actionButtons)
                btn.SetData(index, _building, HideControls);

            nameLabel.text = _building.Id.ToString();
            if (levelLabel)
                levelLabel.text = _building.Level.ToString();

            var maxDeterioration = _settlement.GetMaxDeterioration();
            deteriorationStatus.gameObject.SetActive(_building.Deterioration > maxDeterioration / 2);
            deteriorationStatus.SetStatus(_building.Deterioration, (int)maxDeterioration);

            var needsWorkers = _building.TurnsToBuild > 0 ||
                               _building.Id is BuildingType.WoodCollector or BuildingType.FoodCollector
                                   or BuildingType.StoneCollector;
            workerStatus.gameObject.SetActive(needsWorkers);
            if (needsWorkers)
                workerStatus.SetCount(_settlement.Get().WorkerAssignment.Count(w => w == _index));

            extractionStatus.gameObject.SetActive(
                _building.Id is BuildingType.StoneCollector
                && _building.TurnsToBuild == 0);
            extractionStatus.SetCount(_building.Extraction);
        }

        private void OnNextTurn()
        {
            if (_building.TurnsToBuild > 0)
                return;

            var workers = _settlement.Get().WorkerAssignment.Count(w => w == _index);

            var diff = _building.Id switch
            {
                BuildingType.FoodCollector => new ResourceDiff()
                {
                    Food = (int)Math.Pow(2, _building.Level) * workers
                },
                BuildingType.WoodCollector => new ResourceDiff()
                {
                    Wood = (int)Math.Pow(2, _building.Level) * workers
                },
                BuildingType.WaterCollector => new ResourceDiff()
                {
                    Water = (int)Math.Pow(2, _building.Level)
                },
                BuildingType.StoneCollector => new ResourceDiff()
                {
                    Stone = Math.Min(_building.Extraction, (int)Math.Pow(2, _building.Level) * workers)
                },
                _ => null
            };

            if (diff != null)
                _resourceDiff.Dispatch(diff, 0, worldAnchor);
        }

        private void HideControls()
        {
            controls.SetActive(false);
        }

        public void ShowControls(bool value)
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
            _nextTurn.Remove(OnNextTurn);
        }
    }
}