using UnityEngine;

public class LeafBehaviour : MonoBehaviour
{
    [SerializeField] private float bendAngle = -10f;
    [SerializeField] private float bendSpeed = 8f;

    private bool fishOn;

    void Update()
    {
        float target = fishOn ? bendAngle : 0f;
        float angle = Mathf.LerpAngle(transform.eulerAngles.z, target, bendSpeed * Time.deltaTime);
        
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (var c in collision.contacts)
            {
                if (Vector3.Dot(c.normal, Vector3.down) > 0.7f)
                {
                    fishOn = true;
                    return;
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            fishOn = false;
        }
    }
}
