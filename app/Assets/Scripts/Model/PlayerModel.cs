using System;
using System.Linq;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class PlayerModel : InjectableObject
    {
        public readonly Signal Updated = new();


        private Player.Accounts.Player _data;

        public void Set(Player.Accounts.Player value)
        {
            _data = value;
            Updated.Dispatch();
        }

        public Player.Accounts.Player Get()
        {
            return _data;
        }
    }
}