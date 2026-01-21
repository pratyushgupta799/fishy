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

    private float evaporate_t;
    private float raise_t;
    private Vector3 startPos;
    
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
        evaporate_t = 0;
        raise_t = 0;
        raised = false;
        raiseTime = time;
        evaporatedHeight = transform.position.y - height;
        startPos = transform.position;
        transform.position = new Vector3(transform.position.x, transform.position.y - height, transform.position.z);
        raiseHeight = transform.position.y + height;
        transform.localScale = originalScale;
        gameObject.SetActive(true);
    }
    
    void Update()
    {
        if (!raised)
        {
            raise_t += Time.deltaTime/raiseTime;
            Mathf.Clamp01(raise_t);
            
            transform.position = Vector3.Lerp(startPos,
                new Vector3(startPos.x, raiseHeight, startPos.z),
                raise_t);
            
            if (raise_t >= 1)
            {
                raised = true;
            }
        }
        else
        {
            if (canEvaporate)
            {
                evaporate_t += Time.deltaTime / timeToEvaporate;
                evaporate_t = Mathf.Clamp01(evaporate_t);

                transform.position = Vector3.Lerp(
                    startPos,
                    new Vector3(startPos.x, evaporatedHeight, startPos.z),
                    evaporate_t);

                transform.localScale = Vector3.Lerp(
                    originalScale,
                    new Vector3(evaporatedScale, originalScale.y, evaporatedScale),
                    evaporate_t);

                if (evaporate_t >= 1f)
                {
                    Debug.Log("Puddle Evaporated");
                    transform.localScale = originalScale;
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
