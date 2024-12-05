using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class PlayerHeroModel
    {
        public Signal Updated = new();
        
        private Hero.Accounts.Hero _data;
        public bool HasData => _data != null;

        public void Set(Hero.Accounts.Hero value)
        {
            _data = value;
            Updated.Dispatch();
        }

        public Hero.Accounts.Hero Get()
        {
            return _data;
        }
    }
}