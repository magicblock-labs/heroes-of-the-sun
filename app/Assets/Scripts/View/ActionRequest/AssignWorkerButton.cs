using System;
using Connectors;
using Model;
using Notifications;
using Settlement.Types;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.ActionRequest
{
    public class AssignWorkerButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private ShowWorkerSelection _showWorkerSelection;
        [Inject] private InteractionStateModel _interaction;

        private int _index;

        public void SetData(int index, Building value)
        {
            if (value == null)
                return;

            var needsWorkers = value.TurnsToBuild > 0 ||
                               value.Id is BuildingType.WoodCollector or BuildingType.FoodCollector
                                   or BuildingType.StoneCollector or BuildingType.GoldCollector;
            gameObject.SetActive(needsWorkers);

            _index = index;
        }

        public async void AssignWorker()
        {
            _interaction.LockInteraction();
            
            var freeWorker = _settlement.GetFreeWorkerIndex();

            if (freeWorker >= 0)
                await _connector.AssignWorker(Math.Max(0, freeWorker), _index);

            else
            {
                _interaction.SelectedBuildingIndex = _index;
                _showWorkerSelection.Dispatch();
            }
        }
    }
}