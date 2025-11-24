using UnityEngine;

public class CollectibleLogic : MonoBehaviour
{
    [Header("Scale Pulse")]
    [SerializeField] private float scaleMagnitude = 0.2f;
    [SerializeField] private float scaleFrequency = 1f;

    [Header("Y Position Pulse")]
    [SerializeField] private float yMagnitude = 0.5f;
    [SerializeField] private float yFrequency = 1f;

    private Vector3 initialScale;
    private Vector3 initialPosition;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.position;
    }

    void Update()
    {
        // Scale pulse
        float scalePulse = Mathf.Sin(Time.time * scaleFrequency * 2f * Mathf.PI) * scaleMagnitude;
        transform.localScale = initialScale + Vector3.one * scalePulse;

        // Y position pulse
        float yPulse = Mathf.Sin(Time.time * yFrequency * 2f * Mathf.PI) * yMagnitude;
        transform.position = initialPosition + Vector3.up * yPulse;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}