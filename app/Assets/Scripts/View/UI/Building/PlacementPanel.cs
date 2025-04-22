using System.Threading.Tasks;
using Connectors;
using Model;
using Notifications;
using Utils.Injection;

namespace View.UI.Building
{
    public class PlacementPanel : BuildingUIPanel
    {
        [Inject] private GridInteractionStateModel _gridInteraction;
        [Inject] private SettlementModel _settlement;
        [Inject] private PlayerSettlementConnector _connector;
        [Inject] private ConfigModel _config;

        [Inject] private ShowWorkerSelection _showWorkerSelection;

        public void OnTryDragStart()
        {
            if (_gridInteraction.State != GridInteractionState.Dragging)
                _gridInteraction.SetState(GridInteractionState.Dragging);
        }

        public void OnTryDragStop()
        {
            _gridInteraction.SetState(GridInteractionState.Idle);
        }

        public async void OnSubmit()
        {
            if (_gridInteraction.ValidPlacement && _gridInteraction.SelectedBuildingType.HasValue)
            {
                var freeWorkerIndex = _settlement.GetFreeWorkerIndex();

                if (freeWorkerIndex == -1)
                    _gridInteraction.SelectedBuildingIndex = _settlement.Get().Buildings.Length;

                await _connector.Build(
                    (byte)_gridInteraction.CellPosX,
                    (byte)_gridInteraction.CellPosZ,
                    (byte)_gridInteraction.SelectedBuildingType,
                    freeWorkerIndex);

                if (freeWorkerIndex == -1)
                    _showWorkerSelection.Dispatch();

                _gridInteraction.FinishPlacement();
            }
        }

        public void OnCancel()
        {
            _gridInteraction.FinishPlacement();
        }
    }
}