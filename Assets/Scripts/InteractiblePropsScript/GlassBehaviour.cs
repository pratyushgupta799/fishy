using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class GlassBehaviour : MonoBehaviour, IInteractible
{
    private bool isDirty = false;
    private Rigidbody rb;

    [SerializeField] private GameObject waterMesh;
    [SerializeField] private GameObject puddleMesh;
    [SerializeField] private GameObject puddleCenter;
    [SerializeField] private GameObject surfaceMesh;
    [SerializeField] private float maxSlopeAngle = 10f;
    [SerializeField] private float puddleHeightOffset = 0.2f;
    [SerializeField] private float puddleRaiseDuration = 0.5f;

    
    private GameObject droppedPuddle;

    [SerializeField, Range(0f, 180f)] private float minTiltToSpill = 70f;
    [SerializeField] private int checkpointIndex = 1;

    private bool hasSpilled;

    private Vector3 defaultPos;
    private Quaternion defaultRot;

    [Header("Fall Snap")] 
    [SerializeField] private bool fallSnapEnabled = false;
    [SerializeField] private float rotationSnapStartFrom = 30f;
    [SerializeField] private Transform targetFall;

    [Header("SFX")]
    [SerializeField] private AudioClip glassFall;
    [SerializeField] private AudioClip waterSpillSFX;

    // 🔹 ADDED — Physics snap settings
    [Header("Physics Rotation Snap")]
    [SerializeField] private float minRotToSnap = 30f;
    [SerializeField] private float snapTorque = 5f;
    [SerializeField] private float snapAngularDamping = 2f;

    public int CheckpointIndex => checkpointIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultPos = transform.position;
        defaultRot = transform.rotation;
    }

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

    void FixedUpdate()
    {
        if(hasSpilled || !fallSnapEnabled)
            return;
        
        Vector3 euler = rb.rotation.eulerAngles;

        float x = euler.x > 180f ? euler.x - 360f : euler.x;
        float z = euler.z > 180f ? euler.z - 360f : euler.z;

        if (Mathf.Abs(x) < minRotToSnap && Mathf.Abs(z) < minRotToSnap)
            return;

        ApplyYSnapTorque();
    }

    void ApplyYSnapTorque()
    {
        // Debug.Log("Snapping rotation");
        
        Vector3 current = Vector3.ProjectOnPlane(transform.up, Vector3.up).normalized;
        Vector3 target = GetSnapDirection();

        float angle = Vector3.SignedAngle(current, target, Vector3.up);

        Vector3 torque =
            Vector3.up * angle * Mathf.Deg2Rad * snapTorque
            - Vector3.up * rb.angularVelocity.y * snapAngularDamping;

        rb.AddTorque(torque, ForceMode.Acceleration);
    }

    Vector3 GetSnapDirection()
    {
        Vector3 f = GetForwardFlat().normalized;
        Vector3 r = GetRightFlat().normalized;

        Vector3[] dirs =
        {
            f, -f, r, -r,
            (f + r).normalized,
            (f - r).normalized,
            (-f + r).normalized,
            (-f - r).normalized 
        };

        Vector3 current = Vector3.ProjectOnPlane(transform.up, Vector3.up).normalized;

        Vector3 best = dirs[0];
        float bestDot = -1f;

        foreach (var d in dirs)
        {
            float dot = Vector3.Dot(current, d);
            // Debug.Log("Dot of vector " + current + " with vector " + d + ": " + dot);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = d;
            }
        }

        Debug.Log("Best Snap Direction: " + best);
        return best;
    }

    void TrySpill()
    {
        if (hasSpilled) return;
        hasSpilled = true;

        waterMesh.SetActive(false);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.AudioPlayOneShotAt(glassFall, transform.position);
        }

        Ray ray = new Ray(puddleCenter.transform.position, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 200f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (angle <= maxSlopeAngle)
            {
                if (AudioManager.Instance != null)
                { 
                    AudioManager.Instance.AudioPlayOneShotAt(waterSpillSFX, transform.position);
                }
                droppedPuddle = Instantiate(
                    puddleMesh,
                    hit.point + Vector3.down * puddleHeightOffset,
                    Quaternion.identity
                );

                StartCoroutine(PuddleRaise());
                return;
            }
        }
    }

    private IEnumerator PuddleRaise()
    {
        Vector3 start = droppedPuddle.transform.position;
        Vector3 end = start + Vector3.up * puddleHeightOffset;
        float t = 0f;

        while (t < puddleRaiseDuration)
        {
            t += Time.deltaTime;
            droppedPuddle.transform.position =
                Vector3.Lerp(start, end, t / puddleRaiseDuration);
            yield return null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Define small thresholds to ignore "noise"
        float posThreshold = 0.01f; // 1cm
        float rotThreshold = 0.05f;  // 0.5 degrees

        float distMoved = Vector3.Distance(transform.position, defaultPos);
        float angleChanged = Quaternion.Angle(transform.rotation, defaultRot);

        if (distMoved > posThreshold || angleChanged > rotThreshold)
        {
            SetDirty();
        }
    }

    public void RestoreState()
    {
        if (CheckPointManager.Instance.CurrentCheckpointIndex <= checkpointIndex)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = defaultPos;
            transform.rotation = defaultRot;

            if (droppedPuddle != null)
                Destroy(droppedPuddle.gameObject);

            waterMesh.SetActive(true);
            hasSpilled = false;
        }
    }

    public void ApplyDirty()
    {
        isDirty = false;
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

    Vector3 GetForwardFlat()
    {
        var forward = targetFall.forward;
        forward.y = 0;
        return forward;
    }
    
    Vector3 GetRightFlat()
    {
        var right = targetFall.right;
        right.y = 0;
        return right;
    }
    
    void OnDrawGizmos()
    {
        try
        {
            if (rb == null) rb = GetComponent<Rigidbody>();

            // 1. Calculate the 'current' vector exactly how your snapping logic sees it
            Vector3 current = Vector3.ProjectOnPlane(transform.up, Vector3.up).normalized;

            // 2. Draw the Current Heading Ray
            Gizmos.color = Color.red;
            // We draw it slightly higher (Vector3.up * 0.5f) so it doesn't z-fight with the floor/gizmos
            Vector3 startPos = transform.position;
            Gizmos.DrawRay(startPos, current * 2.5f);

            // 3. Optional: Draw a small sphere at the tip to make it look like a pointer
            Gizmos.DrawSphere(startPos + current * 2.5f, 0.1f);

            Vector3 f = GetForwardFlat().normalized;
            Vector3 r = GetRightFlat().normalized;

            Vector3[] dirs =
            {
                f, -f, r, -r,
                (f + r).normalized,
                (f - r).normalized,
                (-f + r).normalized,
                (-f - r).normalized
            };

            Color[] colors =
            {
                Color.blue, // forward
                Color.red, // back
                Color.green, // right
                Color.yellow, // left
                Color.cyan, // forward-right
                Color.magenta, // forward-left
                Color.gray, // back-right
                Color.white // back-left
            };

            for (int i = 0; i < dirs.Length; i++)
            {
                Gizmos.color = colors[i];
                Gizmos.DrawRay(transform.position, dirs[i]);
            }
        }
        catch (UnassignedReferenceException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            
        }
    }
}
