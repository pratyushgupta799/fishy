using Unity.Cinemachine;
using UnityEditor.PackageManager;
using UnityEngine;


public class CamSnapper : MonoBehaviour
{
    [SerializeField] private CinemachineCamera SnapCam;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Cam snapper zone entered");
            SnapCam.Priority = 1;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Cam snapper zone exited");
            FishyEvents.OnCamSnapZoneExit?.Invoke();
            SnapCam.Priority = -1;
        }
    }
}

