using UnityEngine;

public class GuitarStandBehaviour : MonoBehaviour
{
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CollisionUtils.HitByWithVelocity(collision, "Player", 2f))
        {
            rb.isKinematic = false;
        }
    }
}
