using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public GameObject bulletPrefab; // this prefab needs a NetworkObject component now
    public Transform bulletSpawn;
    public float bulletVelocity = 30;

    void Update()
    {
        if (!IsOwner) return; // only fire when it's your own weapon

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireServerRpc(bulletSpawn.position, bulletSpawn.rotation);
        }
    }

    [ServerRpc]
    private void FireServerRpc(Vector3 spawnPos, Quaternion spawnRot)
    {
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);

        Collider shooterCollider = GetComponentInParent<Collider>(); // looks up the hierarchy, not just this object
        Collider bulletCollider = bullet.GetComponent<Collider>();

        if (shooterCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, shooterCollider);
        }

        bullet.GetComponent<NetworkObject>().Spawn(true);
        bullet.GetComponent<Rigidbody>().AddForce(spawnRot * Vector3.forward * bulletVelocity, ForceMode.Impulse);
    }
}