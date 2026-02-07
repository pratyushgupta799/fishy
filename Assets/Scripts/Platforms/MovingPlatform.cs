using System;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Vector3 playerScale;
    private Transform playerTransform;
    private bool parented;

    void Update()
    {
        if (parented)
        {
            playerTransform.localScale = playerScale;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            parented = true;
            playerScale = other.transform.localScale;
            playerTransform = other.transform;
            other.transform.SetParent(transform);
            Debug.Log("Player parented to platform");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.transform.SetParent(null);
            parented = false;
        }
    }
}
