using UnityEngine;

public class PuddleBehaviour : MonoBehaviour
{
    private bool canEvaporate;
    private float timeToEvaporate;
    private float evaporatedHeight;
    private float evaporatedScale;

    private Vector3 originalScale;

    private float raiseTime;
    private bool raised = true;
    private float raiseHeight;
    
    private void Awake()
    {
        originalScale = transform.localScale;
    }
    
    public void SetEvaporate(float time, float minScale)
    {
        canEvaporate = true;
        timeToEvaporate = time;
        evaporatedScale = minScale * transform.localScale.x;
    }

    public void Raise(float time, float height)
    {
        // Debug.Log("Raising puddle");
        raised = false;
        raiseTime = time;
        evaporatedHeight = transform.position.y - height;
        transform.position = new Vector3(transform.position.x, transform.position.y - height, transform.position.z);
        raiseHeight = transform.position.y + height;
        transform.localScale = originalScale;
        gameObject.SetActive(true);
    }
    
    void Update()
    {
        if (!raised)
        {
            transform.position = Vector3.Lerp(transform.position,
                new Vector3(transform.position.x, raiseHeight, transform.position.z),
                Time.deltaTime / raiseTime);
            
            if (Mathf.Abs(transform.position.y - raiseHeight) < 0.01f)
            {
                raised = true;
            }
        }
        else
        {
            if (canEvaporate)
            {
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x, evaporatedHeight, transform.position.z),
                    Time.deltaTime / timeToEvaporate);

                transform.localScale = Vector3.Lerp(transform.localScale,
                    new Vector3(evaporatedScale, transform.localScale.y, evaporatedScale),
                    Time.deltaTime / timeToEvaporate
                );
                
                if (Mathf.Abs(transform.position.y - evaporatedHeight) < 0.01f)
                {
                    transform.localScale = originalScale;
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
