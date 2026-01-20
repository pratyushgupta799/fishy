using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;


public class CamSnapper : MonoBehaviour
{
    [SerializeField] private CinemachineCamera SnapCam;
    [SerializeField] private FishControllerRB fishy;

    [SerializeField] private bool lockForward;
    [SerializeField] private bool lockSideway;
    [SerializeField] private bool lockUpward;

    private void Awake()
    {
        fishy = GameObject.FindGameObjectWithTag("Player").GetComponent<FishControllerRB>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Cam snapper zone entered");
            SnapCam.Priority = 1;
            fishy.LockMovement(lockForward, lockSideway, lockUpward);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Cam snapper zone exited");
            FishyEvents.OnCamSnapZoneExit?.Invoke();
            SnapCam.Priority = -1;
            fishy.UnlockMovement();
        }
    }
}