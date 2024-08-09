using Unity.AI.Navigation;
using UnityEngine;

namespace View
{
    [RequireComponent(typeof(NavMeshSurface))]
    public class RuntimeNavmeshBake : MonoBehaviour
    {
        void Start()
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }
}
