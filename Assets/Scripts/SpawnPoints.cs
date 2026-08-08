using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    public static SpawnPoints Instance;

    public Transform spawnPointA;
    public Transform spawnPointB;

    void Awake()
    {
        Instance = this;
    }
}