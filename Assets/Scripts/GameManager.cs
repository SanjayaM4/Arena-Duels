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
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null) continue;

            Health health = playerObj.GetComponent<Health>();
            if (health != null) health.currentHealth.Value = health.maxHealth;
        }

        RematchClientRpc();
    }

    [ClientRpc]
    private void RematchClientRpc()
    {
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
    }
}