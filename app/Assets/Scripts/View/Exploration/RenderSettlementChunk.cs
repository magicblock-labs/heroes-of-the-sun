using Model;
using UnityEngine;

namespace View.Exploration
{
    public class RenderSettlementChunk : MonoBehaviour
    {
        
        public void Create(Settlement.Accounts.Settlement value)
        {
            foreach (var settlementDataRenderer in GetComponentsInChildren<IDisplaySettlementData>())
                settlementDataRenderer.SetData(value);
        }
    }
}