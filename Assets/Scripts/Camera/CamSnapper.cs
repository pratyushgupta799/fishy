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

    private void OnEnable()
    {
        FishyEvents.LastCheckpointLoaded += OnCheckPointChanged;
    }

    private void OnDisable()
    {
        FishyEvents.LastCheckpointLoaded -= OnCheckPointChanged;
    }

    private void OnCheckPointChanged()
    {
        Debug.Log("OnCheckPointChanged Event");
        playerOverlap = 0;
        Debug.Log("Player overlap " + playerOverlap);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOverlap++;
            Debug.Log("Player overlap: " + playerOverlap);
            // Debug.Log("Cam snapper zone entered");
            SnapCam.Priority = 1;
            fishy.LockMovement(lockForward, lockSideway, lockUpward);
            if (snapperCenter == SnapperCenter.x)
            {
                fishy.SnapToX(other.transform.position.x);
            }
            else if (snapperCenter == SnapperCenter.z)
            {
                fishy.SnapToZ(other.transform.position.z);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOverlap--;
            Debug.Log("Player overlap: " + playerOverlap);
            if (playerOverlap > 0)
            {
                return;
            }
            Debug.Log("Cam snapper zone exited");
            FishyEvents.OnCamSnapZoneExit?.Invoke();
            SnapCam.Priority = -1;
            fishy.UnlockMovement();
        }
    }
}