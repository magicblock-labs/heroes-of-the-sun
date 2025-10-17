using Model;
using TMPro;
using UnityEngine;
using Utils.Injection;

namespace View.Exploration
{
    public class DisplayHomePointer : InjectableBehaviour
    {
        [Inject] private PlayerHeroModel _playerHero;
        [Inject] private PlayerModel _playerModel;

        [SerializeField] private RectTransform homePointer;
        [SerializeField] private RectTransform playerPointer;
        [SerializeField] private TMP_Text distanceLabel;

        private Vector2 _settlementPosition;

        private void Start()
        {
            var settlement = _playerModel.Get().Settlements[0];
            _settlementPosition = new Vector2(settlement.X * 96 - 1, settlement.Y * 96 - 1);
        }

        void Update()
        {
            if (_playerHero?.Get() == null)
                return;

            var heroPosition = _playerHero.ImmediatePosition;

            var diff = _settlementPosition - heroPosition;
            var diffUnits = diff * ConfigModel.CellSize;
            
            distanceLabel.text = $"{diffUnits.magnitude:0}m";

            var maxDimension = Mathf.Max(diffUnits.x, diffUnits.y);
            var overflow = maxDimension / 128;
            if (overflow > 1)
                diffUnits /= overflow;
            
            homePointer.anchoredPosition = diffUnits;
            playerPointer.rotation = Quaternion.AngleAxis(-_playerHero.ImmediateRotation, Vector3.forward);
        }
    }
}