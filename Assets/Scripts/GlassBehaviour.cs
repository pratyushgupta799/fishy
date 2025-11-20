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
        float tiltX = Mathf.Abs(transform.rotation.eulerAngles.x);
        float tiltZ = Mathf.Abs(transform.rotation.eulerAngles.z);

        bool tilted = (tiltX > 70f && tiltX < 110f) || (tiltZ > 70f && tiltZ < 110f);

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
