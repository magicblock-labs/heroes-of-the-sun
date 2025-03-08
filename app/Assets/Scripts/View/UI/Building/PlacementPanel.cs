using System.Threading.Tasks;
using Connectors;
using Model;
using Notifications;
using Utils.Injection;

namespace View.UI.Building
{
    public class PlacementPanel : BuildingUIPanel
    {
        [Inject] private InteractionStateModel _interaction;
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private ConfigModel _config;

        [Inject] private ShowWorkerSelection _showWorkerSelection;

        public void OnTryDragStart()
        {
            if (_interaction.State != InteractionState.Dragging)
                _interaction.SetState(InteractionState.Dragging);
        }

        public void OnTryDragStop()
        {
            _interaction.SetState(InteractionState.Idle);
        }

        public async void OnSubmit()
        {
            if (_interaction.ValidPlacement && _interaction.SelectedBuildingType.HasValue)
            {
                var freeWorkerIndex = _settlement.GetFreeWorkerIndex();

                if (freeWorkerIndex == -1)
                    _interaction.SelectedBuildingIndex = _settlement.Get().Buildings.Length;

                await _connector.Build(
                    (byte)_interaction.CellPosX,
                    (byte)_interaction.CellPosZ,
                    (byte)_interaction.SelectedBuildingType,
                    freeWorkerIndex);

                if (freeWorkerIndex == -1)
                    _showWorkerSelection.Dispatch();

                _interaction.FinishPlacement();
            }
        }

        public void OnCancel()
        {
            _interaction.FinishPlacement();
        }
    }
}