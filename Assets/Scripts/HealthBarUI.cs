using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider myHealthBar;
    public Slider enemyHealthBar;

    private bool bound = false;

    void Update()
    {
        if (!bound) TryBindBars();
    }

    void TryBindBars()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count < 2) return; // wait for both players

        int foundCount = 0;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null) continue;

            Health health = playerObj.GetComponent<Health>();
            if (health == null) continue;

            Slider targetBar = playerObj.IsOwner ? myHealthBar : enemyHealthBar;

            targetBar.maxValue = health.maxHealth;
            targetBar.value = health.currentHealth.Value;

            health.currentHealth.OnValueChanged += (oldVal, newVal) =>
            {
                targetBar.value = newVal;
            };

            foundCount++;
        }

        if (foundCount == 2) bound = true; // stop checking once both are wired up
    }

    public void ResetBinding()
    {
        bound = false;
    }
}