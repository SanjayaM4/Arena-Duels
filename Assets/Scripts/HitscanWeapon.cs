using Unity.Netcode;
using UnityEngine;

public class HitscanWeapon : WeaponBase
{
    public float range = 100f;
    public LayerMask hitMask;

    private Vector3 lastHitPoint; // used for the tracer visual
    public LineRenderer tracerLine;
    public float tracerDuration = 0.05f;

    protected override void OnFireServer(Vector3 spawnPos, Quaternion spawnRot)
    {
        Ray ray = new Ray(spawnPos, spawnRot * Vector3.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            lastHitPoint = hit.point;

            if (hit.collider.CompareTag("Target"))
            {
                NetworkObject hitNetworkObject = hit.collider.GetComponentInParent<NetworkObject>();

                // skip self-damage, same pattern as Bullet.cs
                if (hitNetworkObject != null && hitNetworkObject.OwnerClientId != OwnerClientId)
                {
                    Health targetHealth = hit.collider.GetComponentInParent<Health>();
                    if (targetHealth != null)
                    {
                        targetHealth.TakeDamage(damage);
                    }
                }
            }
        }
        else
        {
            lastHitPoint = spawnPos + ray.direction * range; // no hit - tracer goes max range
        }

        ShowTracerClientRpc(spawnPos, lastHitPoint);
    }


    [ClientRpc]
    private void ShowTracerClientRpc(Vector3 start, Vector3 end)
    {
        if (tracerLine != null)
        {
            StopAllCoroutines(); // in case of rapid-fire, restart cleanly rather than stacking
            StartCoroutine(DrawTracer(start, end));
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