using UnityEngine;

public class GlassBehaviour : MonoBehaviour, IInteractible
{
    private bool isDirty = false;
    
    private Rigidbody rb;

    [SerializeField] private GameObject waterMesh;
    [SerializeField] private GameObject puddleMesh;
    [SerializeField] private GameObject puddleCenter;
    [SerializeField] private GameObject surfaceMesh;

    private GameObject droppedPuddle;
    
    [SerializeField, Range(0f, 180f)] private float minTiltToSpill = 70f;

    [SerializeField] private int checkpointIndex = 1;

    private bool hasSpilled;

    private Vector3 defaultPos;
    private Quaternion defaultRot;
    
    public int CheckpointIndex => checkpointIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultPos = this.transform.position;
        defaultRot = this.transform.rotation;
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
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore)) 
        {
            Debug.Log("water spilled on " + hit.transform.name);
            droppedPuddle = Instantiate(puddleMesh, hit.point, Quaternion.identity);
            waterMesh.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SetDiry(CheckPointManager.Instance.CurrentCheckpointIndex);
        }
    }

    public void RestoreState()
    {
        if (CheckPointManager.Instance.CurrentCheckpointIndex <= checkpointIndex)
        {
            Debug.Log("Current checkpoint: " + CheckPointManager.Instance.CurrentCheckpointIndex +
                      " <= Glass checkpoint: " + checkpointIndex);
            transform.position = defaultPos;
            transform.rotation = defaultRot;

            if (droppedPuddle != null)
            {
                Destroy(droppedPuddle.gameObject);
            }

            waterMesh.SetActive(true);
            hasSpilled = false;
        }
    }

    public void ApplyDirty()
    {
        isDirty = false;
    }
    
    public void SetDiry(int index)
    {
        if (isDirty) return;
        isDirty = true;
        
        CheckPointManager.Instance.AddChangedPrefab(gameObject);
    }
}
