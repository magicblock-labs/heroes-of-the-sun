using System;
using System.Collections.Generic;
using System.Linq;
using Settlement.Types;
using Solana.Unity.Wallet;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class SmartObjectModel : InjectableObject
    {
        private readonly Dictionary<Vector2Int, PublicKey> _data = new();
        
        public bool HasSmartObjectAt(Vector2Int position)
        {
            return _data.ContainsKey(position);
        }

        public bool HasSmartObjectNextTo(Vector2Int position, out PublicKey entity, int threshold = 2)
        {
            if (!_data.Keys.Any(pos => (position - pos).sqrMagnitude <= threshold))
            {
                entity = null;
                return false;
            }

            var pos = _data.Keys.First(pos => (position - pos).sqrMagnitude <= threshold);
            entity = _data[pos];
            return true;
        }

        public void Set(Vector2Int location, PublicKey entity)
        {
            _data[location] = entity;
        }
    }
}