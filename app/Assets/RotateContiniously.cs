using UnityEngine;

public class RotateContiniously : MonoBehaviour
{
    [SerializeField] private float speed = 1;

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);
    }
}
