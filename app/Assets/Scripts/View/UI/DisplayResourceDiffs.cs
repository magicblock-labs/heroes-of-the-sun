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

    private void OnResourceDiff(ResourceDiff resources, float gold, Transform anchor)
    {
        Instantiate(prefab, transform).SetData(resources, gold, anchor);
    }

    void OnDestroy()
    {
        _notification.Remove(OnResourceDiff);
    }
}
