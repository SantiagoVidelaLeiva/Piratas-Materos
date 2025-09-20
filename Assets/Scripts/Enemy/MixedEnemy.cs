using UnityEngine;

public class MixedEnemy : EnemyControllerBase
{
    // Campos ya serializados en el inspector (hijos de AttackBase que implementan IAttackStrategy)
    [SerializeField] private MonoBehaviour primaryAttackBehaviour;
    [SerializeField] private MonoBehaviour secondaryAttackBehaviour;

    private IAttackStrategy _primary, _secondary;

    protected override void Awake()
    {
        base.Awake();
        _primary = primaryAttackBehaviour as IAttackStrategy;
        _secondary = secondaryAttackBehaviour as IAttackStrategy;
    }

    protected override void TickDanger(bool seesPlayer, Vector3 seenPos)
    {

        agent.speed = chaseSpeed;
        
        if (seesPlayer)
        {
            _lastKnownPos = seenPos;


            float d = Vector3.Distance(transform.position, seenPos);
            var current = (d < 5f) ? _primary : _secondary;
            agent.stoppingDistance = current.StopDistance;
            if (d > agent.stoppingDistance + 0.05f)
            {
                agent.SetDestination(seenPos);
            }
            else 
            {
                agent.ResetPath();
                if (current.CanAttack(player, seenPos))
                    current.Attack(player, seenPos);
            }
        }
        else
        {
            agent.SetDestination(_lastKnownPos);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                SetState(EnemyState.Suspicious);
            }
        }
    }
}
