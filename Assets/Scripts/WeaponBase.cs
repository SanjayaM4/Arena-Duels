using Unity.Netcode;
using UnityEngine;

public abstract class WeaponBase : NetworkBehaviour
{
    public Transform bulletSpawn;
    public float fireRate = 0.2f;
    public bool isAutomatic = false;
    public int damage = 10;

    public AudioSource audioSource;
    public AudioClip shootSound;
    public Animator animator;

    private float nextFireTime = 0f;

    void Update()
    {
        if (!IsOwner) return;

        bool wantsToFire = isAutomatic ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        if (wantsToFire && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            RequestFireServerRpc(bulletSpawn.position, bulletSpawn.rotation);
        }
    }

    [ServerRpc]
    protected void RequestFireServerRpc(Vector3 spawnPos, Quaternion spawnRot)
    {

        OnFireServer(spawnPos, spawnRot); // each weapon type implements this differently
        FireFeedbackClientRpc();
    }

    // Each weapon subtype implements what actually happens when fired - server side, authoritative
    protected abstract void OnFireServer(Vector3 spawnPos, Quaternion spawnRot);

    [ClientRpc]
    protected void FireFeedbackClientRpc()
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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("PlayerModel"));
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}