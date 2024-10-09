using Model;
using Notifications;
using Settlement.Types;
using Utils.Injection;
using BuildingType = Settlement.Types.BuildingType;

namespace View.ActionRequest
{
    public class ResearchButton : InjectableBehaviour, IBuildingActionButton
    {
        [Inject] private SettlementModel _settlement;
        [Inject] private ShowResearch _showResearch;
        [Inject] private InteractionStateModel _interaction;

        private int _index;

        public void SetData(int index, Building value)
        {
            if (value == null)
                return;

            gameObject.SetActive(value.TurnsToBuild<=0 && value.Id == BuildingType.Research);
        }

        public void ShowResearch()
        {
            _interaction.LockInteraction();
            _showResearch.Dispatch();
        }
    }
}