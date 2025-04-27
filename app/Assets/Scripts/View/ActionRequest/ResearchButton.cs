using System;
using Model;
using Notifications;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.ActionRequest
{
    public class ResearchButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ShowResearch _showResearch;
        [Inject] private GridInteractionStateModel _gridInteraction;

        private int _index;
        private Action _callback;

        public void SetData(int index, Settlement.Types.Building value, Action callback)
        {
            if (value == null)
                return;
            

            _callback = callback;

            gameObject.SetActive(value.TurnsToBuild <= 0 && value.Id == BuildingType.Research);
        }

        public void ShowResearch()
        {
            _callback?.Invoke();

            _gridInteraction.LockInteraction();
            _showResearch.Dispatch();
        }
    }
}