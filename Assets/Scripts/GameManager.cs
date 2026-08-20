using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestRematchServerRpc()
    {
        Debug.Log("RequestRematchServerRpc RECEIVED on server");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null)
            {
                Debug.LogWarning("PlayerObject null for client " + client.ClientId);
                continue;
            }

            Health health = playerObj.GetComponent<Health>();
            if (health != null)
            {
                health.currentHealth.Value = health.maxHealth;
                health.ResetDeathState();
                Debug.Log("Reset health for client " + client.ClientId);
            }
        }

        RematchClientRpc();
        Debug.Log("RematchClientRpc sent");
    }

    [ClientRpc]
    private void RematchClientRpc()
    {
        Debug.Log("RematchClientRpc RECEIVED on client, IsOwner: " + IsOwner);

        GameUIManager.Instance.ShowGameplay();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayer != null)
        {
            PlayerMovement movement = localPlayer.GetComponent<PlayerMovement>();
            if (movement != null) movement.ResetToSpawn();
        }
    }

    public void LeaveRoom()
    {

        NetworkManager.Singleton.Shutdown();

        GameUIManager.Instance.ShowMenu();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}