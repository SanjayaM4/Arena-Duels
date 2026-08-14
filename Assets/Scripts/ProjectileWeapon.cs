using Unity.Netcode;
using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    public GameObject bulletPrefab;
    public float bulletVelocity = 30;

    protected override void OnFireServer(Vector3 spawnPos, Quaternion spawnRot)
    {
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.shooterClientId = OwnerClientId;

        Collider shooterCollider = GetComponentInParent<Collider>();
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (shooterCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, shooterCollider);
        }

        bullet.GetComponent<NetworkObject>().Spawn(true);
        bullet.GetComponent<Rigidbody>().AddForce(spawnRot * Vector3.forward * bulletVelocity, ForceMode.Impulse);
    }
}