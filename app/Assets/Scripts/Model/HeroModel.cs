using UnityEngine;
using Utils.Injection;

namespace Model
{
    [Singleton]
    public class HeroModel
    {
        public Vector2Int Location = - Vector2Int.one;
    }
}