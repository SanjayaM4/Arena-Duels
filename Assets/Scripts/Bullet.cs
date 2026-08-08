using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name + " (tag: " + collision.gameObject.tag + ")");
    

        if (!IsServer) return;
        if (!NetworkObject.IsSpawned) return;

    if (collision.gameObject.CompareTag("Target"))
    {
        Health targetHealth = collision.gameObject.GetComponentInParent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(10);
        }
    }

        NetworkObject.Despawn();
    }
}