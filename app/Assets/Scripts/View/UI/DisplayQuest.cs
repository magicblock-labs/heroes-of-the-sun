using System;
using System.Threading.Tasks;
using Connectors;
using Model;
using Notifications;
using Settlement.Types;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

public enum QuestType
{
    Build,
    Upgrade,
    Store,
    Research,
    Faith
}

[Serializable]
public class QuestData
{
    public int id;
    public QuestType type;
    public int targetType;
    public int targetValue = 1;

    public int rewardType;
    public ushort rewardValue;

    // ReSharper disable once InconsistentNaming
    public int? dependsOn;
}

public class DisplayQuest : InjectableBehaviour
{
    [Inject] SettlementModel _settlement;
    [Inject] PlayerSettlementConnector _connector;

    [Inject] StartFtueSequence _startFtueSequence;
    [Inject] ResourceDiffNotification _resourceDiff;


    [SerializeField] private Image typeIcon;
    [SerializeField] private Sprite[] questTypeIcons;
    [SerializeField] private Text title;
    [SerializeField] private Outline infoOutline;

    [SerializeField] private Image progressFill;
    [SerializeField] private Text progressLabel;

    [SerializeField] private Button claimButton;
    [SerializeField] private Text claimText;
    [SerializeField] private Image claimResourceIcon;
    [SerializeField] private Sprite[] resourceIcons;
    [SerializeField] private Transform rewardAnchor;

    private QuestData _data;
    private uint _progress;

    public bool SetData(QuestData data, uint progress)
    {
        _data = data;
        _progress = progress;

        return Redraw();
    }

    private bool Redraw()
    {
        typeIcon.sprite = questTypeIcons[(int)_data.type];
        title.text = _data.type switch
        {
            QuestType.Build => $"Build a {(BuildingType)_data.targetType}",
            QuestType.Upgrade => $"Upgrade a {(BuildingType)_data.targetType}",
            QuestType.Store => $"Have {_data.targetValue} of {(Resource)_data.targetType}",
            QuestType.Research => $"Research {(SettlementModel.ResearchType)_data.targetType}",
            QuestType.Faith => $"Have faith of {_data.targetValue}",
            _ => throw new ArgumentOutOfRangeException()
        };

        var clampedProgress = Mathf.Clamp(_progress, 0, _data.targetValue);
        progressFill.fillAmount = clampedProgress / _data.targetValue;
        progressLabel.text = $"{_progress}/{_data.targetValue}";

        claimButton.interactable = _progress >= _data.targetValue;
        claimText.text = $"Claim x{_data.rewardValue}";
        claimResourceIcon.sprite = resourceIcons[_data.rewardType % 4];

        return _progress >= _data.targetValue;
    }

    public void OnInfoClick()
    {
        _startFtueSequence.Dispatch(_data);
    }

    public void OnClaimClick()
    {
        _ = ClaimAsync();
    }

    private async Task ClaimAsync()
    {
        claimButton.interactable = false;
        await _connector.ClaimQuest(_data.id);
        claimButton.interactable = _progress >= _data.targetValue;
        var diffValue = new ResourceDiff();

        switch (_data.rewardType)
        {
            case (int)Resource.Food:
                diffValue.Food += _data.rewardValue;
                break;
            case (int)Resource.Wood:
                diffValue.Wood += _data.rewardValue;
                break;
            case (int)Resource.Water:
                diffValue.Water += _data.rewardValue;
                break;
            case (int)Resource.Stone:
                diffValue.Stone += _data.rewardValue;
                break;
        }

        _resourceDiff.Dispatch(diffValue, 0, rewardAnchor);
    }
}