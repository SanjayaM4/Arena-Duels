using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public GameObject fpsGunObject;  // camera-view gun, has ProjectileWeapon/HitscanWeapon script
        public GameObject handGunObject; // visual-only gun attached to hand bone
    }

    public WeaponSlot[] weapons;

    // Server-authoritative, same pattern as Health - only server actually changes it, syncs to everyone
    public NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        currentWeaponIndex.OnValueChanged += (oldVal, newVal) => UpdateActiveWeapon(newVal);
        UpdateActiveWeapon(currentWeaponIndex.Value); // apply correct state immediately on spawn
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) RequestSwitchServerRpc(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) RequestSwitchServerRpc(1);
        // add more KeyCode.AlphaX lines as you add more weapons
    }

    [ServerRpc]
    private void RequestSwitchServerRpc(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        currentWeaponIndex.Value = index;
    }

    private void UpdateActiveWeapon(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            bool isActive = (i == index);
            weapons[i].fpsGunObject.SetActive(isActive);
            weapons[i].handGunObject.SetActive(isActive);
        }
    }
}