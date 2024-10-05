using System;
using Service;
using Settlement.Types;
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
        
        public BuildingType? SelectedBuildingType;

        public InteractionState State => _state;

        public bool ValidPlacement;

        public int CellPosX;
        public int CellPosZ;
        
        public int SelectedBuildingIndex = -1;
        private float _lockInteractionUntil;

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
            SelectedBuildingType = null;
            Updated.Dispatch();
        }

        public void OnActionRequested()
        {
            _lockInteractionUntil = Time.time + 0.1f;
        }

        public bool CanInteract => _lockInteractionUntil < Time.time;
    }
}