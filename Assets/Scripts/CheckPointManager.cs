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
    
    private int currentCheckpoint = 0;

    [SerializeField] [ReadOnly] private List<GameObject> changedPrefab;
    
    public int CurrentCheckpointIndex { get { return currentCheckpoint; } private set { currentCheckpoint = value; } }
    
    [SerializeField] private TextMeshProUGUI checkPointText;

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
        
        checkPointText.text = currentCheckpoint.ToString();
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
                    checkPointText.text = currentCheckpoint.ToString();
                }
            }
        }
    }
    
    public void LoadLastCheckpoint()
    {
        for (int i = 0; i < changedPrefab.Count; i++)
        {
            if (changedPrefab[i].GetComponent<StateManager>() != null)
            {
                changedPrefab[i].GetComponent<StateManager>().RestoreDefault();
            }
            else
            {
                Debug.Log(changedPrefab[i].name + " doesnt have a StateManager component");
            }
        }
        fishy.transform.position = checkPoint[currentCheckpoint].transform.position;
        fishy.transform.rotation = checkPoint[currentCheckpoint].transform.rotation;
    }
    
    public void AddChangedPrefab(GameObject prefab)
    {
        changedPrefab.Add(prefab);
    }
}