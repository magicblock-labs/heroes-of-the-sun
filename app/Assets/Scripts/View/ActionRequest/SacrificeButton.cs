using System;
using Connectors;
using Model;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.ActionRequest
{
    public class SacrificeButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private GridInteractionStateModel _gridInteraction;

        private int _index;
        private Action _callback;

        public void SetData(int index, Settlement.Types.Building value, Action callback)
        {

            _callback = callback;
            
            if (value == null)
                return;

            gameObject.SetActive(value.TurnsToBuild <= 0 && value.Id == BuildingType.Altar);
        }

        public async void Sacrifice()
        {
            _callback?.Invoke();
            
            _gridInteraction.LockInteraction();

            var workerIndex = _settlement.GetFreeWorkerIndex();

            if (workerIndex < 0)
                workerIndex = 0;


            await _connector.Sacrifice(workerIndex);
        }
    }
}