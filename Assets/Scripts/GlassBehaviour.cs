using UnityEngine;

public class GlassBehaviour : MonoBehaviour
{
    private Rigidbody rb;
    private bool canDrop;
    [SerializeField] private float torqueForce;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        canDrop = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && canDrop)
        {
            Debug.Log("hit");
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 dir = (transform.position - hitPoint).normalized;
            rb.AddTorque(Vector3.Cross(Vector3.up, dir).normalized * torqueForce, ForceMode.Impulse);
            canDrop = false;
        }
    }
}
