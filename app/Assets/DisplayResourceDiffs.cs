using Notifications;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;


public class DisplayResourceDiffs : InjectableBehaviour
{
    [Inject] ResourceDiffNotification _notification;

    [SerializeField] private DisplayResourceDiff prefab;
    private Camera _camera;

    void Start()
    {
        _notification.Add(OnResourceDiff);
        _camera = Camera.main;
    }

    private void OnResourceDiff(ResourceDiff resources, float gold, Vector3 pos)
    {
        var screenPos = _camera.WorldToScreenPoint(pos);
        Instantiate(prefab, transform).SetData(resources, gold, screenPos);
    }

    void OnDestroy()
    {
        _notification.Remove(OnResourceDiff);
    }
}
