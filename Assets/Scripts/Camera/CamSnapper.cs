using System;
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

    private float playerOverlap = 0;

    [Serializable]
    public enum SnapperCenter
    {
        x,
        z,
        none
    }

    [SerializeField] private SnapperCenter snapperCenter;

    private void Awake()
    {
        fishy = GameObject.FindGameObjectWithTag("Player").GetComponent<FishControllerRB>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOverlap++;
            // Debug.Log("Cam snapper zone entered");
            SnapCam.Priority = 1;
            fishy.LockMovement(lockForward, lockSideway, lockUpward);
            if (snapperCenter == SnapperCenter.x)
            {
                fishy.SnapToX(other.transform.position.x);
            }
            else
            {
                fishy.SnapToZ(other.transform.position.z);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerOverlap >= 1)
            {
                playerOverlap--;
                return;
            }
            // Debug.Log("Cam snapper zone exited");
            FishyEvents.OnCamSnapZoneExit?.Invoke();
            SnapCam.Priority = -1;
            fishy.UnlockMovement();
        }
    }
}