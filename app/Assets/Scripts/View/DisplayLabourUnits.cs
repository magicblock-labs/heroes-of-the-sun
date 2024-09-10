using Model;
using UnityEngine;
using UnityEngine.Serialization;
using Utils.Injection;
using View;

public class DisplayLabourUnits : InjectableBehaviour
{
    [Inject] private SettlementModel _settlement;
    [SerializeField] private LabourUnit labourUnitPrefab;

    public void Start()
    {
        _settlement.Updated.Add(Redraw);
        if (_settlement.HasData)
            Redraw();
    }

    private void Redraw()
    {
        var allocation = _settlement.Get().LabourAllocation;
        for (var i = transform.childCount; i < allocation.Length; i++)
            Instantiate(labourUnitPrefab, transform).SetIndex(i);
    }

    public void OnDestroy()
    {
        _settlement.Updated.Remove(Redraw);
    }
}