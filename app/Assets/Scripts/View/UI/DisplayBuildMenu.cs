using System;
using Service;
using UnityEngine;

namespace View.UI
{
    public class DisplayBuildMenu : MonoBehaviour
    {
        [SerializeField] private BuildingSelector prefab;

        void Start()
        {
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                if (type is not (BuildingType.None or BuildingType.TownHall))
                    Instantiate(prefab, transform).SetData(type);
            }
        }
    }
}