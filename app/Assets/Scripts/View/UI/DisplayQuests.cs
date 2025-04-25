using System;
using DG.Tweening;
using Model;
using Notifications;
using UnityEngine;
using UnityEngine.UI;
using Utils.Injection;

public class DisplayQuests : InjectableBehaviour
{
    [Inject] private ConfigModel _configModel;
    [Inject] private SettlementModel _settlement;
    [Inject] private ShowFtuePrompt _showFtue;

    [SerializeField] private Transform questsContainer;
    [SerializeField] private Toggle ctaButton;
    [SerializeField] private Text ctaStatus;
    [SerializeField] private Transform arrow;
    [SerializeField] private DisplayQuest questPrefab;

    void Start()
    {
        SetOpen(false);
        Redraw();
        
        _settlement.Updated.Add(Redraw);
        _showFtue.Add(OnShowFtue);
    }

    private void OnShowFtue(FtuePrompt[] _)
    {
        SetOpen(false);
    }

    public void SetOpen(bool value)
    {
        questsContainer.gameObject.SetActive(value);
        ctaButton.isOn = value;
        arrow.DOLocalRotate(new Vector3(0, 0, value ? 180f : 0f), .2f);
    }


    private void Redraw()
    {
        foreach (Transform child in questsContainer)
            Destroy(child.gameObject);

        var claimable = 0;
        var total = 0;
        foreach (var (_, quest) in _configModel.GetFirstUnclaimedQuests(0))
        {
            total++;
            if (Instantiate(questPrefab, questsContainer).SetData(quest, _settlement.GetQuestProgress(quest)))
                claimable++;
        }


        ctaStatus.text = $"Quests: {claimable}/{total}";
    }

    private void OnDestroy()
    {
        _settlement.Updated.Remove(Redraw);
        _showFtue.Remove(OnShowFtue);
    }
}