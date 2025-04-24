using System.Collections.Generic;
using Settlement.Types;
using UnityEngine;
using Utils.Injection;

namespace Model
{
    [Singleton]
    public class CtaRegister
    {
        private readonly Dictionary<int, Transform> _data = new();

        public void Add(Transform transform, CtaTag tag, BuildingType? buildingType = null)
        {
            _data[GetCompositeId(tag, buildingType)] = transform;
        }

        private static int GetCompositeId(CtaTag tag, BuildingType? buildingType = null)
        {
            var result = (int)tag;
            if (buildingType.HasValue)
                result |= ((int)buildingType + 1) << 8;

            return result;
        }

        public void Remove(CtaTag tag, BuildingType? buildingType = null)
        {
            // _data.Remove(GetCompositeId(tag, buildingType));
        }

        public Transform Get(CtaTag tag, BuildingType? buildingType = null)
        {
            return _data.GetValueOrDefault(GetCompositeId(tag, buildingType));
        }
    }
}