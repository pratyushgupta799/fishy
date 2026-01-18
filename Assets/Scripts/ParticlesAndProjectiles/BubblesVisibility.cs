using UnityEngine;

public class BubblesVisibility : MonoBehaviour
{
    private ParticleSystem ps;
    private bool insideWater = false;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            insideWater = true;
            ps.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            insideWater = false;
            ps.Stop();
        }
    }
}
