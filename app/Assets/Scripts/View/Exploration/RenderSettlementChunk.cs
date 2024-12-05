using TMPro;
using UnityEngine;
using View.Building;

namespace View.Exploration
{
    public class RenderSettlementChunk : MonoBehaviour
    {
        [SerializeField] private TMP_Text keyLabel;

        public void Create(Settlement.Accounts.Settlement value)
        {
            keyLabel.text = value.Owner.ToString()[..4];

            foreach (var settlementDataRenderer in GetComponentsInChildren<IDisplaySettlementData>())
                settlementDataRenderer.SetData(value);
        }
    }
}