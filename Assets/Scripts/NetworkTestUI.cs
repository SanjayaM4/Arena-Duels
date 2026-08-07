using Unity.Netcode;
using UnityEngine;

public class NetworkTestUI : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 200));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        }
        else
        {
            GUILayout.Label("Mode: " + (NetworkManager.Singleton.IsHost ? "Host" : "Client"));
        }

        GUILayout.EndArea();
    }
}