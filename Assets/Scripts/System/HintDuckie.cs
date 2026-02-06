using UnityEngine;

public class HintDuckie : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Level1Hint.Instance.DuckieCollected();
        }
    }
}
