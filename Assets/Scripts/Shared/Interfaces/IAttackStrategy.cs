using UnityEngine;

public interface IAttackStrategy
{
    float StopDistance { get; }              
    bool CanAttack(Transform target, Vector3 seenPos);
    void Attack(Transform target, Vector3 seenPos);
}

