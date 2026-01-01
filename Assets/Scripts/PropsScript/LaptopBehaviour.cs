using System;
using UnityEngine;

public class LaptopBehaviour : MonoBehaviour
{
    [SerializeField] private MeshRenderer screenMeshRenderer;
    [SerializeField] private Material deadScreenMaterial;
    [SerializeField] private Light laptopLight;
    
    private bool isDamaged = false;

    private void OnTriggerStay(Collider other)
    {
        if (!isDamaged)
        {
            if (other.gameObject.tag == "Water")
            {
                Debug.Log("collision striked water");
                var mats = screenMeshRenderer.materials;
                laptopLight.color = Color.blue;
                mats[1] = deadScreenMaterial;
                screenMeshRenderer.materials = mats;
                isDamaged = true;
            }
        }
    }
}
