using Unity.Netcode;
using UnityEngine;

public class HitscanWeapon : WeaponBase
{
    public float range = 100f;
    public LayerMask hitMask;

    private Vector3 lastHitPoint; // used for the tracer visual
    public LineRenderer tracerLine;
    public float tracerDuration = 0.05f;
    public GameObject hitEffectPrefab;

    protected override void OnFireServer(Vector3 spawnPos, Quaternion spawnRot)
    {
        Ray ray = new Ray(spawnPos, spawnRot * Vector3.forward);

        RaycastHit[] hits = Physics.RaycastAll(ray, range, hitMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool didHit = false;
        RaycastHit validHit = default;

        foreach (RaycastHit h in hits)
        {
            NetworkObject hitNetworkObject = h.collider.GetComponentInParent<NetworkObject>();

            // skip anything that belongs to yourself - keep checking further along the ray
            if (hitNetworkObject != null && hitNetworkObject.OwnerClientId == OwnerClientId)
            {
                continue;
            }

            validHit = h;
            didHit = true;
            break; // first valid (non-self) hit wins
        }

        if (didHit)
        {
            lastHitPoint = validHit.point;

            if (validHit.collider.CompareTag("Target"))
            {
                Health targetHealth = validHit.collider.GetComponentInParent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage);
                }
            }
        }
        else
        {
            lastHitPoint = spawnPos + ray.direction * range;
        }

        ShowTracerClientRpc(spawnPos, lastHitPoint, didHit);
    }


    [ClientRpc]
    private void ShowTracerClientRpc(Vector3 start, Vector3 end, bool didHit)
    {
        if (tracerLine != null)
        {
            StopAllCoroutines();
            StartCoroutine(DrawTracer(start, end));
        }

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, end, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(effect, 2f);
        }
    }

    private System.Collections.IEnumerator DrawTracer(Vector3 start, Vector3 end)
    {
        tracerLine.enabled = true;
        tracerLine.SetPosition(0, start);
        tracerLine.SetPosition(1, end);

        yield return new WaitForSeconds(tracerDuration);

        tracerLine.enabled = false;
    }
}