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
            const int chunkSize = 24;
            var i = 0;
            foreach (var loot in value.Loots)
            {
                i++;
                
                var lootLocation = new Vector2Int(loot.X, loot.Y);
                var chunkLocation = new Vector2Int((int)Math.Floor(lootLocation.x / (float)chunkSize),
                    (int)Math.Floor(lootLocation.y / (float)chunkSize));

                if (chunkLocation.x % 2 == 0 && chunkLocation.y % 2 == 0) //on settlement
                {
                    switch (i % 3)
                    {
                        case 0:
                            lootLocation += new Vector2Int(chunkSize, 0);
                            break;
                        case 1:
                            lootLocation += new Vector2Int(0, chunkSize);
                            break;
                        case 2:
                            lootLocation += new Vector2Int(chunkSize, chunkSize);
                            break;
                    }
                }
                
                loot.X = lootLocation.x;
                loot.Y = lootLocation.y;
            }
            
            
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