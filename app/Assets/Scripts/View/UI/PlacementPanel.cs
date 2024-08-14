using Model;
using Service;
using UnityEngine;
using Utils.Injection;

namespace View.UI
{
    public class PlacementUI : BuildingUI
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
                if (!await _connector.PlaceBuilding((byte)_interaction.CellPosX, (byte)_interaction.CellPosZ,
                        (byte)_interaction.SelectedBuildingType))
                    Debug.LogError("cant place building!");

                await _connector.ReloadData();

                if (_config.Buildings.ContainsKey(_interaction.SelectedBuildingType))
                    _interaction.FinishPlacement();
                else
                    _interaction.CellPosX++;
            }
        }

        public void OnCancel()
        {
            _interaction.FinishPlacement();
        }
    }
}