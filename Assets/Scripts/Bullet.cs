using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!NetworkObject.IsSpawned) return; // safety guard against the exact race you hit

        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("hit " + collision.gameObject.name + " !");
            // apply damage here
        }

        NetworkObject.Despawn();
    }
}