using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public static CheckPointManager Instance;
    
    [SerializeField] private GameObject fishy;
    [SerializeField] private List<GameObject> checkPointTrigger;
    [SerializeField] private List<GameObject> checkPoint;

    [SerializeField] private int loadFromCheckpoint = -1;
    
    private int currentCheckpoint = 0;

    [SerializeField] [ReadOnly] private List<GameObject> changedPrefab;
    
    public int CurrentCheckpointIndex { get { return currentCheckpoint; } private set { currentCheckpoint = value; } }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        
        FishyEvents.OnCheckpointChanged?.Invoke(currentCheckpoint);
    }

    private void Start()
    {
        if (loadFromCheckpoint > 0)
        {
            LoadFromCheckpoint(loadFromCheckpoint - 1);
        }
    }

    public void SetCheckPoint(GameObject checkpointTrigger)
    {
        for (int i = 0; i < checkPointTrigger.Count; i++)
        {
            if (checkPointTrigger[i] == checkpointTrigger)
            {
                if (currentCheckpoint < i)
                {
                    currentCheckpoint = i;
                    FishyEvents.OnCheckpointChanged?.Invoke(currentCheckpoint);
                }
            }
        }
    }

    private void LoadFromCheckpoint(int checkpoint)
    {
        fishy.GetComponent<FishControllerRB>().SnapFishyTo(checkPoint[checkpoint].transform.position,
            checkPoint[checkpoint].transform.rotation);
        FishyEvents.LastCheckpointLoaded?.Invoke();
        
        for (int i = 0; i < changedPrefab.Count; i++)
        {
            if (changedPrefab[i].GetComponent<StateManager>() != null)
            {
                try
                {
                    changedPrefab[i].GetComponent<StateManager>().RestoreDefault();
                }
                catch (Exception e)
                {
                    Debug.LogError("Problem in " + changedPrefab[i].transform.name + ": " + e.Message);
                }
            }
            else
            {
                Debug.Log(changedPrefab[i].name + " doesnt have a StateManager component");
            }
        }
    }
    
    public void LoadLastCheckpoint()
    {
        LoadFromCheckpoint(currentCheckpoint);
    }
    
    public void AddChangedPrefab(GameObject prefab)
    {
        changedPrefab.Add(prefab);
    }
}