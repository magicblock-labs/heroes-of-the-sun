using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;

namespace View.UI.Building
{
    [Serializable]
    public enum BuildingFilter
    {
        ResourceCollection,
        Storage,
        Special
    }

    public class DisplayBuildMenu : InjectableBehaviour
    {
        [Inject] private NavigationContextModel _nav;
        [Inject] private ConfigModel _config;

        [SerializeField] private BuildingSelector prefab;


        void Start()
        {
            Redraw();
        }

        public void SetFilter(int value)
        {
            _nav.BuildingFilter = (BuildingFilter)value;
            Redraw();
        }

        private void Redraw()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            foreach (var type in _config.BuildingTypeMapping.Where(b => b.Value == _nav.BuildingFilter)
                         .Select(b => b.Key))
            {
                if (type is not BuildingType.TownHall)
                    Instantiate(prefab, transform).SetData(type);
            }
        }
    }
}