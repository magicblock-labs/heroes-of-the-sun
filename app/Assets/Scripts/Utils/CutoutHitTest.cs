using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class CutoutHitTest : Image
    {
        [SerializeField] private RectTransform[] cutout;

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            // If the base Image would return false, return false
            if (!base.IsRaycastLocationValid(screenPoint, eventCamera))
                return false;

            // Convert screen point to local coordinates
            foreach (var c in cutout)
            {
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(c, screenPoint, eventCamera, out var p)) continue;
            
                if (c.rect.Contains(p))
                    return false;
            }

            return true;
        }
    }
}