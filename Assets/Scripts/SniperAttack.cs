using UnityEngine;

public class SniperAttack : AttackBase
{
    [Header("High-Ranged")]
    [SerializeField] private float aimTime = 0.2f; // opcional: tiempo de apuntado
    private float _aimUntil;

    protected override void DoAttack(Transform target, Vector3 seenPos)
    {
        // opcional: pequeño retardo de apuntado
        if (Time.time < _aimUntil)
            return;

        _aimUntil = Time.time + aimTime;

        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.6f;
        Vector3 dir = (seenPos - origin).normalized;

        // Sniper: sin spread, más rango/daño/cooldown en el prefab
        if (Physics.Raycast(origin, dir, out var hit, maxRange, hitMask))
        {
            Debug.Log($"[SNIPER] Headshot a {hit.collider.name} por {damage}");
            // TODO: hit.collider.GetComponent<Health>()?.ApplyDamage(damage);
        }
        else
        {
            Debug.Log("[SNIPER] Falló el tiro");
        }
    }
}