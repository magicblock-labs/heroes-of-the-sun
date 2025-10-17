using UnityEngine;

namespace View.Building
{
    public interface IDisplaySettlementData
    {
        void SetData(Settlement.Accounts.Settlement value, Vector2Int offset);
    }
}