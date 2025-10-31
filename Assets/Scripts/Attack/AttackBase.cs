using UnityEngine;

public abstract class AttackBase : MonoBehaviour, IAttackStrategy
{
    [Header("Data")]
    [SerializeField] protected AttackData _data;  // <- asignás el SO acá en el inspector

    [Header("Common Refs")]
    [SerializeField] protected Transform firePoint;     // origen del ataque (mano/arma)
    [SerializeField] protected LayerMask hitMask;       // capas válidas para raycast

    protected float _nextAttackTime;

    // EnemyController usa esto para saber a qué distancia frenar.
    public virtual float StopDistance => _data ? _data.maxRange : 1.5f;

    protected virtual void Awake()
    {
        // Por si te olvidás de setear el mask en el inspector:
        hitMask = LayerMask.GetMask("Player");
    }

    // -------- Lógica común --------
    public virtual bool CanAttack(Transform target, Vector3 seenPos)
    {
        // cooldown + rango
        if (Time.time < _nextAttackTime) return false;
        return IsInRange(seenPos);
    }

    public void Attack(Transform target, Vector3 seenPos)
    {
        if (!CanAttack(target, seenPos)) return;

        // seteamos próximo ataque usando el cooldown de la data
        _nextAttackTime = Time.time + _data.cooldown;

        DoAttack(target, seenPos);
    }

    protected virtual bool IsInRange(Vector3 targetPos)
    {
        float range = _data ? _data.maxRange : 8f;
        return Vector3.Distance(transform.position, targetPos) <= range;
    }

    protected abstract void DoAttack(Transform target, Vector3 seenPos);
}
