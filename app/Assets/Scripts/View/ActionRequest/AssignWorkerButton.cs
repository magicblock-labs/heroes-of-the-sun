using System;
using Connectors;
using Model;
using Notifications;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.ActionRequest
{
    public class AssignWorkerButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private ShowWorkerSelection _showWorkerSelection;
        [Inject] private GridInteractionStateModel _gridInteraction;

        private int _index;
        private Action _callback;

        public void SetData(int index, Settlement.Types.Building value, Action callback)
        {
            if (value == null)
                return;
            
            
            _callback = callback;

            var needsWorkers = value.TurnsToBuild > 0 ||
                               value.Id is BuildingType.WoodCollector or BuildingType.FoodCollector
                                   or BuildingType.StoneCollector ;
            gameObject.SetActive(needsWorkers);

            _index = index;
        }

        public async void AssignWorker()
        {
            _callback?.Invoke();

            _gridInteraction.LockInteraction();

            var freeWorker = _settlement.GetFreeWorkerIndex();

            if (freeWorker >= 0)
                await _connector.AssignWorker(Math.Max(0, freeWorker), _index);

            else
            {
                _gridInteraction.SelectedBuildingIndex = _index;
                _showWorkerSelection.Dispatch();
            }
        }
    }
}