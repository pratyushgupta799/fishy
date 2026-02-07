using DG.Tweening;
using UnityEngine;

public class AquariumBehaviour : MonoBehaviour
{
    [SerializeField] private Collider impactHitCollider;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private Mesh brokenGlassMesh;
    [SerializeField] private GameObject waterStream;
    [SerializeField] private float waterStreamBlinkTime;
    [SerializeField] private Transform puddleLocation;
    [SerializeField] private GameObject aquariumWater;
    [SerializeField] private float aquariumWaterFallDelta;
    
    private MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        waterStream.SetActive(false);
    }

    void OnCollisionEnter(Collision col)
    {
        // make sure the collider that got hit is the one we care about
        if (col.GetContact(0).thisCollider != impactHitCollider)
            return;

        // check if it's the player
        if (!col.collider.CompareTag("Player"))
            return;
        
        Debug.Log("Aquarium hit by player");

        if (col.gameObject.GetComponent<FishControllerRB>().IsDashing())
        {
            meshFilter.mesh = brokenGlassMesh;
            impactHitCollider.enabled = false;
            
            meshCollider.sharedMesh = meshFilter.mesh;
            waterStream.SetActive(true);
            
            PuddleManager.Instance.RaiseAquariumPuddle(puddleLocation.position, waterStreamBlinkTime, 0.02f);
            aquariumWater.transform.DOMoveY(
                aquariumWater.transform.position.y - aquariumWaterFallDelta,
                waterStreamBlinkTime
            );

            DOVirtual.DelayedCall(waterStreamBlinkTime, () =>
            {
                waterStream.SetActive(false);
            });
        }
    }
}