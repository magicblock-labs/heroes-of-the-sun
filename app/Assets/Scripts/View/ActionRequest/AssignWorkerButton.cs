using System;
using Model;
using Notifications;
using Service;
using Settlement.Types;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.ActionRequest
{
    public class AssignWorkerButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;
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
            _interaction.OnActionRequested();
            
            var freeWorker = _settlement.GetFreeWorkerIndex();

            if (freeWorker >= 0)
            {
                if (await _connector.AssignLabour(Math.Max(0, freeWorker), _index))
                    await _connector.ReloadData();
            }

            else
            {
                _interaction.SelectedBuildingIndex = _index;
                _showWorkerSelection.Dispatch();
            }
        }
    }
}