using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RBState : MonoBehaviour, IInteractible
{
    [SerializeField] private int checkpointIndex = 100;
    
    private Vector3 initPos;
    private Quaternion initRot;

    private bool isDirty = false;
    
    private Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initPos = transform.position;
        initRot =  transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Define small thresholds to ignore "noise"
        float posThreshold = 0.01f; // 1cm
        float rotThreshold = 0.05f;  // 0.5 degrees

        float distMoved = Vector3.Distance(transform.position, initPos);
        float angleChanged = Quaternion.Angle(transform.rotation, initRot);

        if (distMoved > posThreshold || angleChanged > rotThreshold)
        {
            SetDirty();
        }
    }
    
    public void SetDirty()
    {
        if (isDirty) return;
        isDirty = true;
        if (CheckPointManager.Instance != null)
        {
            CheckPointManager.Instance.AddChangedPrefab(gameObject);
        }
    }
    
    public void RestoreState()
    {
        if (CheckPointManager.Instance.CurrentCheckpointIndex <= checkpointIndex)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = initPos;
            transform.rotation = initRot;
        }
    }
}
