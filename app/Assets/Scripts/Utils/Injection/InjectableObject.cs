namespace Utils.Injection
{
    public class InjectableObject
    {   
        public InjectableObject()
        {
            Injector.Instance.Resolve(this);
        }
    }
}