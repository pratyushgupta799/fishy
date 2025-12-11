using UnityEditor.PackageManager;
using UnityEngine;


public class CamSnapper : MonoBehaviour
{
    [SerializeField] private Transform SnapCam;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Cam snapper zone entered");
            FishyEvents.OnCamSnapZoneEntered?.Invoke(SnapCam.position);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FishyEvents.OnCamSnapZoneExit?.Invoke();
        }
    }
}

