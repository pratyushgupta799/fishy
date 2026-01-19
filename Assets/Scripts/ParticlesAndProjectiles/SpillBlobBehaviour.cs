using UnityEngine;

public class SpillBlobBehaviour : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float gravity;
    [SerializeField] private int maxBounces;
    [SerializeField] private float heightOffset = 0.25f;
    [SerializeField] private float raiseTime = 0.5f;
    [SerializeField] private LayerMask Player;
    [SerializeField] private ParticleSystem blobSplash;
    
    private Vector3 velocity;
    private int bounceCount;
    private bool active;
    private Transform lookTarget;
    private float curGravity;
    private ParticleSystem blobSplashInstance;

    private float spillEvaporateTime;

    private void Awake()
    {
        blobSplashInstance = Instantiate(blobSplash);
    }
    
    void SetLookTarget()
    {
        lookTarget = Camera.main.transform;
    }

    public void Init(Vector3 startPos, Vector3 dir, float time)
    {
        curGravity = gravity;
        
        spillEvaporateTime = time;
        
        SetLookTarget();
        transform.position = startPos;
        
        velocity = dir * speed;
        bounceCount = 0;
        active = true;
        
        gameObject.SetActive(true);
    }
    
    void Update()
    {
        if (lookTarget != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - lookTarget.position
            );
        }
        
        if (!active) return;
        
        Vector3 move = velocity * Time.deltaTime;

        if (Physics.Raycast(transform.position, move.normalized, out RaycastHit hit, move.magnitude, ~Player,
                QueryTriggerInteraction.Ignore))
        {
            transform.position = hit.point;
            CheckBounce(hit);
        }
        
        velocity.y -= curGravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    void CheckBounce(RaycastHit hit)
    {
        if (Vector3.Dot(hit.normal, Vector3.up) >= 0.8f)
        {
            // form puddle;
            PuddleManager.Instance.RaiseEvapouratablePuddle(transform.position, raiseTime, heightOffset,
                spillEvaporateTime);
            blobSplashInstance.transform.position = hit.point;
            blobSplashInstance.Play();
            active = false;
            gameObject.SetActive(false);
            return;
        }
        if (bounceCount >= maxBounces)
        {
            gameObject.SetActive(false);
            return;
        }
        velocity = Vector3.Reflect(velocity, hit.normal);
        curGravity += 4f;
        bounceCount++;
    }
}
