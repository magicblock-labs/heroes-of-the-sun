using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class BalanceModel : InjectableObject<BalanceModel>
    {
        public Signal Updated = new();
        
        private uint _data;

        public void Set(uint value)
        {
            _data = value;
            Updated.Dispatch();
        }

        public uint Get()
        {
            return _data;
        }
    }
}