using UnityEngine;

public class SpillBehaviour : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float gravity;
    [SerializeField] private int maxBounces;
    [SerializeField] private LayerMask Player;
    
    private Vector3 velocity;
    private int bounceCount;
    private bool active;
    private Transform lookTarget;
    private float curGravity;
    
    void SetLookTarget()
    {
        lookTarget = Camera.main.transform;
    }

    public void Init(Vector3 startPos, Vector3 dir)
    {
        curGravity = gravity;
        
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
        if (bounceCount >= maxBounces)
        {
            // form puddle;
            active = false;
            return;
        }
        velocity = Vector3.Reflect(velocity, hit.normal);
        curGravity += 4f;
        bounceCount++;
    }
}
