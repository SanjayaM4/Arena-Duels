using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    [HideInInspector] public ulong shooterClientId;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!NetworkObject.IsSpawned) return;

        if (collision.gameObject.CompareTag("Target"))
        {
            NetworkObject hitNetworkObject = collision.gameObject.GetComponentInParent<NetworkObject>();

            // skip damage if the bullet hit its own shooter
            if (hitNetworkObject != null && hitNetworkObject.OwnerClientId == shooterClientId)
            {
                NetworkObject.Despawn();
                return;
            }

            Health targetHealth = collision.gameObject.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(10);
            }
        }

        NetworkObject.Despawn();
    }
}