using Utils.Injection;

namespace Model
{
    [Singleton]
    public class PlayerHeroModel
    {
        private Hero.Accounts.Hero _data;
        public bool HasData => _data != null;

        public void Set(Hero.Accounts.Hero value)
        {
            _data = value;
        }

        public Hero.Accounts.Hero Get()
        {
            return _data;
        }
    }
}