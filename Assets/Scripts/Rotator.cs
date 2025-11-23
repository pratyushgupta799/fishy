using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 axis = Vector3.up;   // Local axis
    public float speed = 50f;           // Speed
    public bool clockwise = true;       // Direction

    void Update()
    {
        float dir = clockwise ? -1f : 1f;
        transform.Rotate(axis * dir * speed * Time.deltaTime, Space.Self);
    }
}