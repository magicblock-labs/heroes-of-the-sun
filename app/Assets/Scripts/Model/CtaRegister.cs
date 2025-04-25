using System.Collections.Generic;
using UnityEngine;
using Utils.Injection;

namespace Model
{
    [Singleton]
    public class CtaRegister
    {
        private readonly Dictionary<int, Transform> _data = new();

        public void Add(Transform transform, CtaTag tag, int? payload = null)
        {
            Debug.Log($"Add {tag} : {payload}");
            _data[GetCompositeId(tag, payload)] = transform;
        }

        private static int GetCompositeId(CtaTag tag, int? payload = null)
        {
            var result = (int)tag;
            if (payload.HasValue)
                result |= (payload.Value + 1) << 8;

            return result;
        }

        public Transform Get(CtaTag tag, int? payload = null)
        {
            return _data.GetValueOrDefault(GetCompositeId(tag, payload));
        }
    }
}