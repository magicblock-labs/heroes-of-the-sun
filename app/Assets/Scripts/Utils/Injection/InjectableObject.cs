namespace Utils.Injection
{
    public class InjectableObject<T>
    {
        private static T _instance;

        public static T Instance =>
            _instance ??= (T)Injector.Instance.GetValue(typeof(T));
        
        public InjectableObject()
        {
            Injector.Instance.Resolve(this);
        }
    }
}