using Model;
using Service;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class PlacementPanel : BuildingUIPanel
    {
        [Inject] private InteractionStateModel _interaction;
        [Inject] private SettlementModel _settlement;
        [Inject] private ProgramConnector _connector;
        [Inject] private ConfigModel _config;

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
            if (_interaction.ValidPlacement)
            {
                if (await _connector.PlaceBuilding(
                        (byte)_interaction.CellPosX,
                        (byte)_interaction.CellPosZ,
                        (byte)_interaction.SelectedBuildingType,
                        _settlement.GetFreeWorkerIndex()))
                    await _connector.ReloadData();

                _interaction.FinishPlacement();
            }
        }

        public void OnCancel()
        {
            _interaction.FinishPlacement();
        }
    }
}