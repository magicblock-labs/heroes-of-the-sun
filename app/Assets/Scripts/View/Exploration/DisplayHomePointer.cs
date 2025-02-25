using Model;
using TMPro;
using UnityEngine;
using Utils.Injection;
using View.Exploration;

public class DisplayHomePointer : InjectableBehaviour
{
    [Inject] private PlayerModel _playerModel;

    [SerializeField] private RectTransform _homePointer;
    [SerializeField] private TMP_Text _distanceLabel;

    [SerializeField] private Camera _minimapCamera;

    private Vector3 _settlementPosition;
    private Transform _heroTransform;

    void Start()
    {
        var settlement = _playerModel.Get().Settlements[0];
        _settlementPosition = new Vector3(settlement.X * 96 - 1, 0, settlement.Y * 96 - 1);
    }

    void Update()
    {
        if (_heroTransform == null)
        {
            var hero = FindFirstObjectByType<PointAndClickMovement>();
            if (hero != null)
                _heroTransform = hero.transform;
            else
                return;
        }

        var viewportPos = _minimapCamera.WorldToViewportPoint(_settlementPosition);
        _distanceLabel.text = $"{(_heroTransform.position - _settlementPosition).magnitude:0}m";

        _homePointer.anchoredPosition = new Vector2(Mathf.Clamp(viewportPos.x, 0, 1), Mathf.Clamp(viewportPos.y, 0, 1)) * 256;
    }
}