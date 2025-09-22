using UnityEngine;

public abstract class AttackBase : MonoBehaviour, IAttackStrategy
{
    [Header("Common Attack Settings")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float cooldown = 0.5f;
    [SerializeField] protected float maxRange = 8f;
    [SerializeField] protected Transform firePoint;     // origen del ataque (mano/arma)
    [SerializeField] protected LayerMask hitMask = ~0;  // capas válidas para raycast (si aplica)
    public virtual float StopDistance => maxRange; // StopDistance siempre devuelve el mismo valor que maxRange

    private float _nextAttackTime;


    public bool CanAttack(Transform target, Vector3 seenPos)
    {
        return Time.time >= _nextAttackTime && IsInRange(seenPos);
    }

    public void Attack(Transform target, Vector3 seenPos)
    {
        if (!CanAttack(target, seenPos)) return;
        _nextAttackTime = Time.time + cooldown;
        DoAttack(target, seenPos);
    }

    protected virtual bool IsInRange(Vector3 targetPos)
    {
        return Vector3.Distance(transform.position, targetPos) <= maxRange;
    }

    // Cada derivada define cómo ataca
    protected abstract void DoAttack(Transform target, Vector3 seenPos);
    
    

    
}
