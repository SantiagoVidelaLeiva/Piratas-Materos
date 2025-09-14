using UnityEngine;

public interface IAttackStrategy
{
    void Attack(Transform target, Vector3 seenPos);
}

