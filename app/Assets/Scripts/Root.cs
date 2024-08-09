using UnityEngine;

public class Root : MonoBehaviour
{
    private static GameObject Instance;
    void Start()
    {
        Instance = gameObject;
    }
}
