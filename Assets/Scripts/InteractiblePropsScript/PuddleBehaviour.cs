using UnityEngine;

public class PuddleBehaviour : MonoBehaviour
{
    private bool canEvaporate;
    private float timeToEvaporate;
    private float evaporatedHeight;

    private float raiseTime;
    private bool raised = true;
    private float raiseHeight;
    
    public void SetEvaporate(float time)
    {
        canEvaporate = true;
        timeToEvaporate = time;
    }

    public void Raise(float time, float height)
    {
        Debug.Log("Raising puddle");
        raised = false;
        raiseTime = time;
        evaporatedHeight = transform.position.y - height;
        transform.position = new Vector3(transform.position.x, transform.position.y - height, transform.position.z);
        raiseHeight = transform.position.y + height;
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
                
                if (Mathf.Abs(transform.position.y - evaporatedHeight) < 0.01f)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
