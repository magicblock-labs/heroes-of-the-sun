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
        [SerializeField] private TMP_Text distanceLabel;
        
        private Vector2 _settlementPosition;

        private void Start()
        {
            var settlement = _playerModel.Get().Settlements[0];
            _settlementPosition = new Vector2(settlement.X, settlement.Y);
        }

        void Update()
        {
            if (_playerHero?.Get() == null)
                return;
            
            var heroPosition = new Vector2(_playerHero.Get().X, Mathf.Abs(_playerHero.Get().Y)); 

            distanceLabel.text = $"{((heroPosition - _settlementPosition) * ConfigModel.CellSize).magnitude:0}m";

            //_homePointer.anchoredPosition = new Vector2(Mathf.Clamp(viewportPos.x, 0, 1), Mathf.Clamp(viewportPos.y, 0, 1)) * 256;
        }
    }
}