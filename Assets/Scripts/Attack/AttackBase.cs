using UnityEngine;

public abstract class AttackBase : MonoBehaviour, IAttackStrategy
{
    [Header("Data")]
    [SerializeField] protected AttackData _data;

    [Header("Common Refs")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected LayerMask hitMask;

    protected float _nextAttackTime;

    public virtual float StopDistance => _data ? _data.maxRange : 1.5f;

    protected virtual void Awake()
    {
        hitMask = LayerMask.GetMask("Player");
    }

    public virtual bool CanAttack(Transform target, Vector3 seenPos)
    {
        if (Time.time < _nextAttackTime) return false;
        return IsInRange(seenPos);
    }

    public void Attack(Transform target, Vector3 seenPos)
    {
        if (!CanAttack(target, seenPos)) return;

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
