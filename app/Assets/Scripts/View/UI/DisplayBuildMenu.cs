using System;
using Service;
using UnityEngine;
using View;

public class DisplayBuildMenu : MonoBehaviour
{
    [SerializeField] private BuildingSelector prefab;

    void Start()
    {
        foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
        {
            if (type != BuildingType.None)
                Instantiate(prefab, transform).SetData(type);
        }
    }
}