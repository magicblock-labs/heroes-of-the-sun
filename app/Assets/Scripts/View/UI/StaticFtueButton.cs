using System;
using Model;
using Notifications;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils.Injection;
using View.UI;

public class StaticFtueButton : InjectableBehaviour, IPointerClickHandler
{
    [SerializeField] private CtaTag ctaTag;
    [Inject] ShowFtuePrompt _showFtue;
    [Inject] CtaRegister _ctaRegister;
     
    public void OnPointerClick(PointerEventData eventData)
    {
        var ftuePrompt = new FtuePrompt
        {
            blocking = false,
            cutoutScreenSpace = GetScreenRect(ctaTag)
        };
        
        switch (ctaTag)
        {
            case CtaTag.HUDFaith:

                ftuePrompt.promptLocation = Vector2Int.left;
                ftuePrompt.promptText = "Your <color=green>Faith</color>\ndepends on food and water stock pile,\nand boosts turns regen and cap";
                
                break;
            case CtaTag.HUDTurns:
                
                ftuePrompt.promptLocation = Vector2Int.left;
                ftuePrompt.promptText = "Your workers build and collect\nresources every <color=green>turn</color>";
                
                break;
            
            case CtaTag.HUDTreasury:
                
                ftuePrompt.promptLocation = Vector2Int.down;
                ftuePrompt.promptText = "If a workers are assigned to a <i>resource collection</i> building,\nthey would collect given resources every <color=green>turn</color>, up to the storage capacity";
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _showFtue.Dispatch(new[]
        {
            ftuePrompt
        });
    }
    
    //todo remove dupe 
    
    private Rect GetScreenRect(CtaTag ctaTag, int? payload = null)
    {
        //get rect transform from register
        var ctaTransform = _ctaRegister.Get(ctaTag, payload);
        if (ctaTransform is not RectTransform rectTransform)
        {
            Debug.LogError($"Failed to get rect transform for CtaTag: {ctaTag}[{payload}]");
            return Rect.zero;
        }

        var canvas = DisplayFtueSequence.FindRenderingCanvas(rectTransform);

        // Get the world corners of the RectTransform
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Calculate the screen bounds
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);

        for (var i = 0; i < 4; i++)
        {
            // For Overlay canvas, world corners are already in screen space
            Vector2 screenPoint =
                canvas.worldCamera == null
                    ? corners[i]
                    : canvas.worldCamera.WorldToScreenPoint(corners[i]);

            // Find min/max for creating our bounding rect
            min.x = Mathf.Min(min.x, screenPoint.x);
            min.y = Mathf.Min(min.y, screenPoint.y);
            max.x = Mathf.Max(max.x, screenPoint.x);
            max.y = Mathf.Max(max.y, screenPoint.y);
        }

        // Create and return the screen rect
        var width = max.x - min.x;
        var height = max.y - min.y;
        return new Rect(min.x + width / 2, min.y + height / 2, width, height);
    }
}
