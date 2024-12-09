using System;
using System.Linq;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;
using Utils.Signal;

namespace Model
{
    [Singleton]
    public class SmartObjectModel : InjectableObject
    {
        public readonly Signal Updated = new();

        private Vector2Int[] _data = { new(-3, 0)};

        public void Set(Vector2Int[] value)
        {
            _data = value;
            Updated.Dispatch();
        }

        public Vector2Int[] Get()
        {
            return _data;
        }
        
        public bool HasSmartObjectAt(Vector2Int position, out int index)
        {
            index = Array.FindIndex(_data, so => position == so);
            return index >= 0;
        }

        public bool HasSmartObjectNextTo(Vector2Int position, out int index, int threshold = 2)
        {
            index = Array.FindIndex(_data, so => (position - so).sqrMagnitude <= threshold);
            return index >= 0;
        }
    }
}