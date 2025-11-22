using UnityEngine;

public class GlassBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private GameObject waterMesh;
    [SerializeField] private GameObject puddleMesh;
    [SerializeField] private GameObject puddleCenter;

    private bool hasSpilled;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float tiltX = transform.rotation.eulerAngles.x;
        tiltX = (tiltX > 180f) ? tiltX - 360f : tiltX;

        float tiltZ = transform.rotation.eulerAngles.z;
        tiltZ = (tiltZ > 180f) ? tiltZ - 360f : tiltZ;

        bool tilted = (tiltX > 70f || tiltX < -70f) || (tiltZ > 70f || tiltZ < -70f);

        if (tilted)
        {
            TrySpill();
        }
    }

    void TrySpill()
    {
        if (hasSpilled) return;
        hasSpilled = true;
        
        Ray ray = new Ray(puddleCenter.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Instantiate(puddleMesh, hit.point, Quaternion.identity);
            waterMesh.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
