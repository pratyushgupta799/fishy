using UnityEngine;

public class AquariumBehaviour : MonoBehaviour
{
    [SerializeField] private Collider impactHitCollider;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private Mesh brokenGlassMesh;
    [SerializeField] private float breakSpeed = 5f;   // your threshold

    private MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
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
        }
    }
}