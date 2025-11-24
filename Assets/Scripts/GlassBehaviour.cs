using UnityEngine;

public class GlassBehaviour : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private GameObject waterMesh;
    [SerializeField] private GameObject puddleMesh;
    [SerializeField] private GameObject puddleCenter;
    [SerializeField] private GameObject surfaceMesh;
    
    [SerializeField, Range(0f, 180f)] private float minTiltToSpill = 70f;

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

        bool tilted = Mathf.Abs(tiltX) >= minTiltToSpill || Mathf.Abs(tiltZ) >= minTiltToSpill;

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
            waterMesh.GetComponent<Collider>().enabled = false;
            surfaceMesh.GetComponent<Collider>().enabled = false;
            waterMesh.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
