using DG.Tweening;
using Notifications;
using Settlement.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayResourceDiff : MonoBehaviour
{
    [SerializeField] private GameObject goldCointainer;
    [SerializeField] private Text goldLabel;

    [SerializeField] private GameObject foodCointainer;
    [SerializeField] private Text foodLabel;

    [SerializeField] private GameObject woodIcon;
    [SerializeField] private Text woodLabel;

    [SerializeField] private GameObject waterIcon;
    [SerializeField] private Text waterLabel;

    [SerializeField] private GameObject stoneIcon;
    [SerializeField] private Text stoneLabel;
    private RectTransform _rect;

    public void SetData(ResourceDiff resource, float gold, Vector3 pos)
    {
        goldCointainer.SetActive(gold != 0);
        goldLabel.text = $"{gold:F2}";
        goldLabel.color = gold > 0 ? Color.green : Color.red;

        if (resource != null)
        {
            foodCointainer.SetActive(resource.Food != 0);
            foodLabel.text = resource.Food > 0 ? $"+{resource.Food}" : $"{resource.Food}";
            foodLabel.color = resource.Food > 0 ? Color.green : Color.red;

            woodIcon.SetActive(resource.Wood != 0);
            woodLabel.text = resource.Wood > 0 ? $"+{resource.Wood}" : $"{resource.Wood}";
            woodLabel.color = resource.Wood > 0 ? Color.green : Color.red;

            waterIcon.SetActive(resource.Water != 0);
            waterLabel.text = resource.Water > 0 ? $"+{resource.Water}" : $"{resource.Water}";
            waterLabel.color = resource.Water > 0 ? Color.green : Color.red;

            stoneIcon.SetActive(resource.Stone != 0);
            stoneLabel.text = resource.Stone > 0 ? $"+{resource.Stone}" : $"{resource.Stone}";
            stoneLabel.color = resource.Stone > 0 ? Color.green : Color.red;
        }

        if (!_rect)
            _rect = GetComponent<RectTransform>();

        _rect.position = pos;
        _rect.gameObject.SetActive(true);
        _rect.DOAnchorPosY(_rect.anchoredPosition.y + 30, 1).OnComplete(() => { Destroy(gameObject); }
        );
    }
}