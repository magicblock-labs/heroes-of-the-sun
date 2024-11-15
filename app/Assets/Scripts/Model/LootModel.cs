using System;
using System.Linq;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class LootModel : InjectableObject
    {
        public readonly Signal Updated = new();

        private Lootdistribution.Accounts.LootDistribution _data;

        public void Set(Lootdistribution.Accounts.LootDistribution value)
        {
            _data = value;
            Updated.Dispatch();
        }

        public Lootdistribution.Accounts.LootDistribution Get()
        {
            return _data;
        }

        public bool HasLootAt(Vector2Int position, out int index)
        {
            index = _data.Loots.ToList().FindIndex(l => l.X == position.x && l.Y == position.y);
            return index >= 0;
        }
    }
}