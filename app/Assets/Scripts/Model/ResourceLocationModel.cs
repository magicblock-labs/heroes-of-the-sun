using System.Collections.Generic;
using Utils.Injection;
using UnityEngine;

namespace Model
{
    public enum ResourceType
    {
        Wood,
        Food
    }

    [Singleton]
    public class ResourceLocationModel
    {
        private Dictionary<ResourceType, List<Vector2Int>> _data = new();

        public void Add(ResourceType type, Vector2Int coordinates)
        {
            if (!_data.ContainsKey(type))
                _data[type] = new List<Vector2Int>();
            
            _data[type].Add(coordinates);
        }

        public List<Vector2Int> Get(ResourceType type)
        {
            return _data[type];
        }

        public void Reset()
        {
            _data.Clear();
        }
    }
}