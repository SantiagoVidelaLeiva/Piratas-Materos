using UnityEngine;

public class MeleeAttack : AttackBase
{
    [Header("Melee")]
    [SerializeField] private LayerMask targetMask;
    protected override void DoAttack(Transform target, Vector3 seenPos)
    {
        if (Vector3.Distance(transform.position, target.position) <= maxRange)
        {
            var dmg = target.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage((float)damage);
                Debug.Log($"[MELEE] Le pegué a {target.name} por {damage} de daño");
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }
}