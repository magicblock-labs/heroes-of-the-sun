using System;
using Service;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Flags]
    public enum InteractionState
    {
        None = 0,
        Idle = 1 << 0,
        Dragging = 1 << 1,
        Panning = 1 << 2,
        Dialog = 1 << 3
    }

    [Singleton]
    public class InteractionStateModel : InjectableObject<InteractionStateModel>
    {
        private InteractionState _state = InteractionState.Idle;
        public Signal Updated = new();
        
        public BuildingType SelectedBuildingType = BuildingType.None;

        public InteractionState State => _state;

        public bool ValidPlacement;

        public int CellPosX;
        public int CellPosZ;
        
        public int SelectedBuildingIndex = -1;

        public void StartPlacement(BuildingType type)
        {
            if (SelectedBuildingType == type)
                return;
            
            SelectedBuildingType = type;
            Updated.Dispatch();
        }
        
        public void SetState(InteractionState value)
        {
            if (_state == value) return;

            _state = value;

            Updated.Dispatch();
        }

        public void SetPlacementLocation(int cellPosX, int cellPosZ, bool valid)
        {

            ValidPlacement = valid;
            CellPosX = cellPosX;
            CellPosZ = cellPosZ;
        }

        public void FinishPlacement()
        {
            SelectedBuildingType = BuildingType.None;
            Updated.Dispatch();
        }
    }
}