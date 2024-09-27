using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Notifications;
using Plugins.Demigiant.DOTween.Modules;
using Service;
using Settlement.Types;
using TMPro;
using UnityEngine;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.UI
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

        [SerializeField] private GameObject controls;

        private readonly Dictionary<RectTransform, Vector2> _actionPositions = new();
        private int _index;

        protected override void Start()
        {
            base.Start();
            controls.SetActive(false);

            foreach (RectTransform child in controls.transform)
                _actionPositions[child] = child.anchoredPosition;
        }

        public void SetData(int index, Building value)
        {
            if (value == null)
                return;

            _index = index;

            nameLabel.text = value.Id.ToString();
            if (levelLabel)
                levelLabel.text = value.Level.ToString();

            //todo max deterioration
            deteriorationStatus.gameObject.SetActive(value.Deterioration > 50);
            deteriorationStatus.SetStatus(value.Deterioration, 127);

            var needsWorkers = value.TurnsToBuild > 0 ||
                               value.Id is BuildingType.WoodCollector or BuildingType.FoodCollector;
            workerStatus.gameObject.SetActive(needsWorkers);
            if (needsWorkers)
                workerStatus.SetCount(_settlement.Get().LabourAllocation.Count(w => w == _index));
        }

        public async void Repair()
        {
            if (await _connector.Repair(_index))
                await _connector.ReloadData();
        }

        public async void Upgrade()
        {
            if (await _connector.Upgrade(_index))
                await _connector.ReloadData();
        }

        public async void AllocateWorker()
        {
            var freeWorker = _settlement.GetFreeWorkerIndex();

            if (freeWorker >= 0)
            {
                if (await _connector.AssignLabour(Math.Max(0, freeWorker), _index))
                    await _connector.ReloadData();
            }

            else {
                _interaction.SelectedBuildingIndex = _index;
                _showWorkerSelection.Dispatch();
            }
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