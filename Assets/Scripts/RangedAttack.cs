using UnityEngine;

public class RangedAttack : AttackBase
{
    [Header("Mid-Ranged")]
    [SerializeField] private float spreadDegrees = 2.5f; // pequeña dispersión
    [SerializeField] private float projectileSpeed = 0f; // 0 = usa raycast hitscan

    protected override void DoAttack(Transform target, Vector3 seenPos)
    {
        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 dir = (seenPos - origin).normalized;

        // aplicar un poco de spread (en ejes locales)
        dir = Quaternion.Euler(Random.Range(-spreadDegrees, spreadDegrees),
                               Random.Range(-spreadDegrees, spreadDegrees),
                               0f) * dir;

        // Hitscan simple con Raycast (si no usás balas físicas)
        if (projectileSpeed <= 0f)
        {
            if (Physics.Raycast(origin, dir, out var hit, maxRange, hitMask))
            {
                Debug.Log($"[MID] Impacto en {hit.collider.name} por {damage}");
                // TODO: hit.collider.GetComponent<Health>()?.ApplyDamage(damage);
            }
            else
            {
                Debug.Log("[MID] Disparo fallido");
            }
        }
        else
        {
            // TODO: Instanciar proyectil y darle velocidad hacia 'dir' (si querés proyectiles físicos)
        }
    }
}