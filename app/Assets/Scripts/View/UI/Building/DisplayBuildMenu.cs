using System;
using System.Collections.Generic;
using Settlement.Types;
using UnityEngine;

namespace View.UI.Building
{
    [Serializable]
    public enum BuildingFilter
    {
        ResourceCollection,
        Storage,
        Special
    }

    public class DisplayBuildMenu : MonoBehaviour
    {
        [SerializeField] private BuildingSelector prefab;

        private BuildingFilter _selectedFilter;

        void Start()
        {
            Redraw();
        }

        public void SetFilter(int value)
        {
            _selectedFilter = (BuildingFilter)value;
            Redraw();
        }

        private void Redraw()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            foreach (var type in GetFilteredBuildings())
            {
                if (type is not BuildingType.TownHall)
                    Instantiate(prefab, transform).SetData(type);
            }
        }

        private IEnumerable<BuildingType> GetFilteredBuildings()
        {
            return _selectedFilter switch
            {
                BuildingFilter.ResourceCollection => new[]
                {
                    BuildingType.WaterCollector,
                    BuildingType.FoodCollector,
                    BuildingType.WoodCollector,
                    BuildingType.StoneCollector,
                },
                BuildingFilter.Storage => new[]
                {
                    BuildingType.WaterStorage,
                    BuildingType.FoodStorage,
                    BuildingType.WoodStorage,
                    BuildingType.StoneStorage,
                },
                BuildingFilter.Special => new[]
                {
                    BuildingType.Altar,
                    BuildingType.Research
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}