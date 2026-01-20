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

    [SerializeField] private string[] bounceTags;
    [SerializeField] private string[] ignoreTags;
    
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

        if (Physics.Raycast(transform.position, move.normalized, out RaycastHit hit, move.magnitude, ~Player))
        {
            transform.position = hit.point;
            CheckBounce(hit);
        }
        
        velocity.y -= curGravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    void CheckBounce(RaycastHit hit)
    {
        foreach (string tag in ignoreTags)
        {
            if (hit.transform.CompareTag(tag))
            {
                return;
            }
        }
        bool wrongTag = false;
        if ((hit.transform.tag == "Water" || hit.transform.tag == "WaterSurface") && velocity.y < 0)
        {
            Debug.Log("Spill destroyed in water");
            blobSplashInstance.transform.position = hit.point;
            blobSplashInstance.Play();
            active = false;
            gameObject.SetActive(false);
            return;
        }
        if ((hit.transform.tag == "Water" || hit.transform.tag == "WaterSurface") && velocity.y >= 0)
        {
            return;
        }
        foreach (string tag in bounceTags)
        {
            if (hit.transform.CompareTag(tag))
            {
                wrongTag = true;
            }
        }
        if (Vector3.Dot(hit.normal, Vector3.up) >= 0.8f && !wrongTag)
        {
            // form puddle;
            PuddleManager.Instance.RaiseEvapouratableSpillPuddle(transform.position, raiseTime, heightOffset,
                spillEvaporateTime);
            blobSplashInstance.transform.position = hit.point;
            blobSplashInstance.Play();
            active = false;
            gameObject.SetActive(false);
            return;
        }
        if (bounceCount >= maxBounces)
        {
            Debug.Log("Spill blob destroyed because of max bounces");
            active = false;
            gameObject.SetActive(false);
            return;
        }
        
        Debug.Log("Spill bounced with " + hit.transform.tag);
        velocity = Vector3.Reflect(velocity, hit.normal);
        curGravity += 4f;
        bounceCount++;
    }
}
