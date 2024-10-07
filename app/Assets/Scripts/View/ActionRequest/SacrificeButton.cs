using System;
using Model;
using Notifications;
using Service;
using Settlement.Types;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.ActionRequest
{
    public class SacrificeButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;
        [Inject] private InteractionStateModel _interaction;

        private int _index;

        public void SetData(int index, Building value)
        {
            if (value == null)
                return;

            gameObject.SetActive(value.TurnsToBuild <= 0 && value.Id == BuildingType.Altar);
        }

        public async void ShowResearch()
        {
            _interaction.OnActionRequested();

            var workerIndex = _settlement.GetFreeWorkerIndex();

            if (workerIndex < 0)
                workerIndex = 0;


            if (await _connector.Sacrifice(workerIndex))
                await _connector.ReloadData();
        }
    }
}