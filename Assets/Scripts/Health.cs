using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public int maxHealth = 100;

    // NetworkVariable automatically syncs its value from server to all clients
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server // only the server can change it - prevents cheating
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }

    // Only ever call this server-side (e.g. from Bullet.cs, which already checks IsServer)
    public void TakeDamage(int amount)
    {
        if (!IsServer) return;

        Debug.Log("TakeDamage called on " + gameObject.name + " for " + amount);

        currentHealth.Value -= amount;

        if (currentHealth.Value <= 0)
        {
            currentHealth.Value = 0;
            HandleDeathClientRpc();
        }
    }

    public void Kill()
    {
        if (!IsServer) return;

        currentHealth.Value = 0;
        HandleDeathClientRpc();
    }

    [ClientRpc]
    private void HandleDeathClientRpc()
    {
        bool localPlayerLost = IsOwner;
        GameUIManager.Instance.ShowEndScreen(!localPlayerLost);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}