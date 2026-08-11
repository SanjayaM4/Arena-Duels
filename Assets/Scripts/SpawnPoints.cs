using UnityEngine;
using Unity.Netcode;

public class SpawnPoints : MonoBehaviour
{
    public static SpawnPoints Instance;

    public Transform spawnPointA;
    public Transform spawnPointB;

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            GameUIManager.Instance.ShowGameplay();
        }
    }

    void Awake()
    {
        Instance = this;
    }
}