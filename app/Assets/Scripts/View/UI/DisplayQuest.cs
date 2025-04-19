using System;
using System.Collections;
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
}

public class DisplayQuest : InjectableBehaviour
{
    [Inject] SettlementModel _settlement;
    
    [Inject] ShowFtuePrompt _showFtue;
    [Inject] HideFtuePrompt _hideFtue;
    
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
    
    private QuestData _data;

    public bool SetData(QuestData data, uint progress)
    {
        _data = data;
        typeIcon.sprite = questTypeIcons[(int)data.type];
        title.text = data.type switch
        {
            QuestType.Build => $"Build a {(BuildingType)data.targetType}",
            QuestType.Upgrade => $"Upgrade a {(BuildingType)data.targetType}",
            QuestType.Store => $"Have {data.targetValue} of {(Resource)data.targetType}",
            QuestType.Research => $"Research {(SettlementModel.ResearchType)data.targetType}",
            QuestType.Faith => $"Have faith of {data.targetValue}",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var clampedProgress = Mathf.Clamp(progress, 0, data.targetValue);
        progressFill.fillAmount = clampedProgress / data.targetValue;
        progressLabel.text = $"{progress}/{data.targetValue}";

        claimButton.interactable = progress >= data.targetValue;
        claimText.text = $"Claim x{data.rewardValue}";
        claimResourceIcon.sprite = resourceIcons[data.rewardType % 4];

        return progress >= data.targetValue;
    }

    public void OnInfoClick()
    {
        StartCoroutine(DisplayTutorial());
    }

    private IEnumerator DisplayTutorial()
    {
        _showFtue.Dispatch(new []
        {
            new FtuePrompt()
            {
                blocking = true, promptLocation = Vector2Int.right, promptText = "Open Build Menu", cutoutScreenSpace = new Rect(200, 120, 160, 160)
            }
        });

        yield return null;
    }

    public void StopTutorial()
    {
        StopAllCoroutines();
        _hideFtue.Dispatch();
    }

    public void OnClaimClick()
    {
        _settlement.ClaimQuestLocalSim(_data.id);
    }
}
