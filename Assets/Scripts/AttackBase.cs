using UnityEngine;

// Clase base para ataques. Implementa el contrato y centraliza cooldown/rango
public abstract class AttackBase : MonoBehaviour, IAttackStrategy
{
    [Header("Common Attack Settings")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float cooldown = 0.5f;
    [SerializeField] protected float maxRange = 8f;
    [SerializeField] protected Transform firePoint;     // origen del ataque (mano/arma)
    [SerializeField] protected LayerMask hitMask = ~0;  // capas válidas para raycast (si aplica)


    

    private float _nextTime;

    protected virtual void Awake()
    {

    }
    public void Attack(Transform target, Vector3 seenPos)
    {
        if (Time.time < _nextTime) return;
        if (!IsInRange(seenPos)) return;

        _nextTime = Time.time + cooldown;
        DoAttack(target, seenPos);
        
    }

    protected virtual bool IsInRange(Vector3 targetPos)
    {
        return Vector3.Distance(transform.position, targetPos) <= maxRange;
    }

    // Cada derivada define cómo ataca
    protected abstract void DoAttack(Transform target, Vector3 seenPos);
    
    

    
}
