using System;
using System.Collections;
using UnityEngine;

public class WateringCanBehaviour : MonoBehaviour
{
    [Serializable]
    private enum RotationAngle
    {
        X,
        Y,
        Z
    };
    
    [SerializeField] private float minAngle = 17f;
    [SerializeField] private RotationAngle angleOfRotation;
    [SerializeField] private GameObject waterSpillMesh;
    [SerializeField] private GameObject waterMesh;
    [SerializeField] private float waterRaiseDuration = 1f;

    private Vector3 originalWaterScale;
    private Rigidbody rigidBody;

    private bool hasSpilled;

    private void Awake()
    {
        waterSpillMesh.SetActive(false);
        waterMesh.SetActive(false);
        rigidBody = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        if (!hasSpilled)
        {
            if (angleOfRotation == RotationAngle.X)
            {
                if (transform.localEulerAngles.x >= minAngle)
                {
                    TrySpill();
                }
            }
            else if (angleOfRotation == RotationAngle.Y)
            {
                if (transform.localEulerAngles.y >= minAngle)
                {
                    TrySpill();
                }
            }
            else
            {
                if (transform.localEulerAngles.z >= minAngle)
                {
                    TrySpill();
                }
            }
        }
    }

    private void TrySpill()
    {
        hasSpilled = true;
        waterSpillMesh.SetActive(true);
        waterMesh.SetActive(true);
        originalWaterScale = waterMesh.transform.localScale;
        waterMesh.transform.localScale =
            new Vector3(waterMesh.transform.localScale.x, 0f, waterMesh.transform.localScale.y);

        FreezeRigidBody();
        
        StartCoroutine(WaterFill());
    }

    private IEnumerator WaterFill()
    {
        Vector3 startScale = waterMesh.transform.localScale;
        Vector3 endScale = originalWaterScale;

        float t = 0f;

        while (t < waterRaiseDuration)
        {
            t += Time.deltaTime;
            waterMesh.transform.localScale = Vector3.Lerp(startScale, endScale, t / waterRaiseDuration);
            yield return null;
        }
        
        waterSpillMesh.SetActive(false);
    }

    private void FreezeRigidBody()
    {
        rigidBody.freezeRotation = true;
    }
}
