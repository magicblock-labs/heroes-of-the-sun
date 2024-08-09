using UnityEngine;

namespace Utils.Injection
{
    public class InjectableBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Injector.Instance.Resolve(this, gameObject.GetInstanceID());
        }
    }
}