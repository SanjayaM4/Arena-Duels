using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float fireRate = 0.5f; // seconds between shots - adjust to taste
    private float nextFireTime = 0f;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public Animator animator;


    void Update()
    {
        if (!IsOwner) return; // only fire when it's your own weapon

        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            FireServerRpc(bulletSpawn.position, bulletSpawn.rotation);
        }
    }

    [ServerRpc]
    private void FireServerRpc(Vector3 spawnPos, Quaternion spawnRot)
    {
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.shooterClientId = OwnerClientId; // record who fired this

        Collider shooterCollider = GetComponentInParent<Collider>();
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (shooterCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, shooterCollider);
        }

        bullet.GetComponent<NetworkObject>().Spawn(true);
        bullet.GetComponent<Rigidbody>().AddForce(spawnRot * Vector3.forward * bulletVelocity, ForceMode.Impulse);

        FireClientRpc();
    }

    [ClientRpc]
    private void FireClientRpc()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
    }
}