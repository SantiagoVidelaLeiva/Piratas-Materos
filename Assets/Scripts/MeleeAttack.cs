using UnityEngine;

public class MeleeAttack : AttackBase
{
    [Header("Melee")]
    [SerializeField] private float hitRadius = 1.2f;

    protected override void DoAttack(Transform target, Vector3 seenPos)
    {
        // chequeo simple de distancia (melee)
        if (Vector3.Distance(transform.position, target.position) <= hitRadius)
        {
            Debug.Log($"[MELEE] Hit por {damage}");
            // TODO: target.GetComponent<Health>()?.ApplyDamage(damage);
        }
        else
        {
            // opcional: step hacia el objetivo, anim, etc.
        }
    }
}