using System.Collections.Generic;
using System.Linq;
using Model;
using TMPro;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

namespace View.Exploration
{
    public class DisplaySmartObjects : InjectableBehaviour
    {
        [Inject] private PlayerHeroModel _playerHero;
        [Inject] private SmartObjectModel _smartObject;

        [SerializeField] private Sprite smartObjectIcon;
        [SerializeField] private Transform container;

        private Dictionary<Vector2Int, RectTransform> _smartObjectsIcons;

        private void Start()
        {
            _smartObjectsIcons = _smartObject.GetLocations().ToDictionary(loc=>loc,loc =>
            {
                var obj = Instantiate(new GameObject($"SmartObject@{loc.x},{loc.y}"), container);
                var img = obj.AddComponent<Image>();
                
                img.sprite = smartObjectIcon;
                img.color = Color.yellow;
                var rect = img.GetComponent<RectTransform>();
                rect.sizeDelta = Vector2.one * 20;
                return rect;
            });
        }

        void Update()
        {
            if (_playerHero?.Get() == null)
                return;

            var heroPosition = _playerHero.ImmediatePosition;

            foreach (var (loc, icon) in _smartObjectsIcons)
            {
                var diff = loc - heroPosition;
                var diffUnits = diff * ConfigModel.CellSize;
            
                var maxDimension = Mathf.Max(diffUnits.x, diffUnits.y);
                var overflow = maxDimension / 128;
                if (overflow > 1)
                    diffUnits /= overflow;
            
                icon.anchoredPosition = diffUnits;
            }

            
        }
    }
}