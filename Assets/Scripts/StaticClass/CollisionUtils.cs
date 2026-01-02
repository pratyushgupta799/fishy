using UnityEngine;

public static class CollisionUtils
{
    public static bool HitByWithVelocity(
        Collision collision,
        string tag,
        float minVelocity
    )
    {
        return collision.gameObject.CompareTag(tag) && 
               collision.relativeVelocity.magnitude >= minVelocity;
    }
}
