using System.Diagnostics.CodeAnalysis;

namespace Utils.Injection
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class InjectableObject
    {
        protected InjectableObject()
        {
            Injector.Instance.Resolve(this);
        }
    }
}