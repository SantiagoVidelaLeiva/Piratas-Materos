using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyControllerBase : MonoBehaviour, IVisionProvider
{
    // ============================
    //            Types
    // ============================
    public enum EnemyState { Patrolling, Suspicious, Danger }

    // ============================
    //        Inspector Fields
    // ============================

    [Header("References")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Transform eyes;
    [SerializeField] protected Transform player;
    [SerializeField] protected Transform nearDetect;
    [SerializeField] private Transform visionPivot;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointTolerance = 2f;

    [Header("Suspicion")]
    [SerializeField] private List<Transform> _suspiciousList;
    [SerializeField] private bool _nearestPointSuspicious = true;
    [SerializeField] private float scanDuration = 5f;
    [SerializeField] private float scanYawAmplitude = 70f;
    [SerializeField] private float scanOscillationsPerSecond = 0.2f;

    [Header("Perception / Vision")]
    [SerializeField] private LayerMask obstacleMask = ~0; // por defecto todo
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float eyesHeight = 1.7f;
    [SerializeField] private float lostSightGrace = 4f;

    [Header("Awareness / Proximity")]
    [SerializeField] private float proximityRadius = 5f;

    [Header("Movement / Speeds")]
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float suspiciousSpeed = 3.0f;
    [SerializeField] protected float chaseSpeed = 4.5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;


    // ============================
    //       Runtime / State
    // ============================

    private EnemyState _state = EnemyState.Patrolling;
    public EnemyState CurrentState => _state;

    private int _patrolIndex;
    private float lastSeenTime;
    protected Vector3 _lastKnownPos;

    private bool _scanActive;
    private float _scanTimer;
    private bool _movingToSuspicionPoint;
    private List<Transform> _pendingSuspicion;

    private float _lastSpeed01;
    private Quaternion _pivotBaseLocalRot;

    // ============================
    //       Components / Cache
    // ============================
    protected IAttackStrategy _iattackStrategy;
    private CharacterHealth _enemyHealth;
    private CharacterHealth health;
    private EnemyAnimator _enemyAnimator;

    // ============================
    //       Animator / Params
    // ============================
    public event Action<EnemyState> OnStateChange;
    public event Action OnEnemyDestroyed;
    public event Action<float> OnSpeed01Changed;
    public event Action<AnimState> OnAnimState;
    public event Action<int> OnAnimTrigger;
    public event Action<int, bool> OnAnimBool;

    static readonly int Shoot_Hash = Animator.StringToHash("ShootBool");
    static readonly int AimBool_Hash = Animator.StringToHash("IsAiming");
    const float moveShootSpeedMax = 0.05f;



    // ============================
    //      Unity Lifecycle
    // ============================
    protected virtual void Awake()
    {
        if (!agent)
            agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
            player = playerObj.transform;

        if (patrolPoints == null)
            patrolPoints = new Transform[0];

        if (!eyes)
            eyes = transform.Find("VisionPivot/Eyes");

        if (!nearDetect)
            nearDetect = transform.Find("VisionPivot/NearDetect");

        if (!visionPivot)
            visionPivot = transform.Find("VisionPivot");

        if (visionPivot)
            _pivotBaseLocalRot = visionPivot.localRotation;

        _iattackStrategy = GetComponent<IAttackStrategy>();

        if (!_enemyHealth)
            _enemyHealth = GetComponent<CharacterHealth>();

        _enemyHealth.OnDamaged += HandleDamaged_EnterDanger;

        NoiseSystem.OnNoise += OnNoiseHeard;

        health = GetComponent<CharacterHealth>();
        if (health != null)
            health.OnDied += HandleDeath;
        else
            Debug.LogError($"[{name}] Falta CharacterHealth");

        if (!_enemyAnimator)
            _enemyAnimator = GetComponentInChildren<EnemyAnimator>(true);

        if (patrolPoints != null && patrolPoints.Length > 0)
            RaiseAnimState(AnimState.Idle);
    }



    void OnEnable()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnDied += HandleDeath;
    }

    void OnDisable()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnDied -= HandleDeath;
    }

    private void OnDestroy()
    {
        NoiseSystem.OnNoise -= OnNoiseHeard;
        OnEnemyDestroyed?.Invoke();

        if (health != null)
            health.OnDied -= HandleDeath;

        health.OnDamaged -= HandleDamaged_EnterDanger;
    }

    private void Update()
    {
        //  Percepción
        bool seesPlayer = TrySeePlayer(out Vector3 seenPos);

        //  FSM
        switch (_state)
        {
            case EnemyState.Patrolling:
                TickPatrolling(seesPlayer, seenPos);
                break;

            case EnemyState.Suspicious:
                TickSuspicious(seesPlayer, seenPos);
                break;

            case EnemyState.Danger:
                TickDanger(seesPlayer, seenPos);
                break;
        }

        //  Anim feedback de velocidad normalizada
        float raw = new Vector3(agent.velocity.x, 0f, agent.velocity.z).magnitude;
        float norm = Mathf.InverseLerp(0f, chaseSpeed, raw);


        if (Mathf.Abs(norm - _lastSpeed01) > 0.01f)
        {
            _lastSpeed01 = norm;
            OnSpeed01Changed?.Invoke(norm);
        }
    }


    // ============================
    //           States
    // ============================

    protected virtual void TickPatrolling(bool seesPlayer, Vector3 seenPos)
    {
        agent.speed = patrolSpeed;

        // Veo al jugador? Paso a Danger
        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            SetState(EnemyState.Danger);
            return;
        }

        // Sin puntos de patrulla -> quieto
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (AgentIsValid())
                agent.isStopped = true;
            return;
        }
        if (patrolPoints.Length == 1)
        {
            Transform onlyPoint = patrolPoints[0];
            if (onlyPoint != null)
            {
                // si todavía no estoy en ese punto, voy hasta ahí
                if (!agent.pathPending &&
                    agent.remainingDistance > waypointTolerance)
                {
                    if (AgentIsValid())
                    {
                        agent.isStopped = false;
                        agent.stoppingDistance = 0f;
                        agent.SetDestination(onlyPoint.position);
                    }

                    // anim caminar mientras voy hacia el punto
                    RaiseAnimState(AnimState.Patrolling);
                }
                else
                {
                    // ya estoy en el punto -> quedate quieto mirando, anim idle
                    if (AgentIsValid())
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                    }

                    RaiseAnimState(AnimState.Idle);
                }
            }
            return;
        }
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            AdvancePatrol();
        }

        float horizontalSpeed = new Vector3(agent.velocity.x, 0f, agent.velocity.z).magnitude;

        if (horizontalSpeed < 0.05f)
        {
            RaiseAnimState(AnimState.Idle);
        }
    }

    protected virtual void TickSuspicious(bool seesPlayer, Vector3 seenPos)
    {
        // Si vuelvo a ver al player -> Danger
        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            SetState(EnemyState.Danger);
            return;
        }

        // ir al punto sospechoso inicial (LKP o ruido)
        if (_movingToSuspicionPoint)
        {
            agent.speed = suspiciousSpeed;

            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                _movingToSuspicionPoint = false;
                BeginScan();
            }
            return;
        }

        // escaneo en el punto
        if (_scanActive)
        {
            bool finished = UpdateScan();
            if (finished)
            {
                // cuando termino de escanear, voy al siguiente punto sospechoso
                Transform next = PopNearest(_pendingSuspicion, transform.position);
                if (next != null)
                {
                    _movingToSuspicionPoint = true;
                    if (AgentIsValid())
                        agent.isStopped = false;
                    agent.updateRotation = true;
                    EndScan();
                    agent.SetDestination(next.position);
                    RaiseAnimState(AnimState.Suspicious);
                }
                else
                {
                    EndScan();
                    RaiseAnimState(AnimState.Idle);
                    SetState(EnemyState.Patrolling);
                }
            }
            return;
        }

        // Fase 3: caminar por lista de puntos sospechosos del inspector
        if (_pendingSuspicion != null && _pendingSuspicion.Count > 0)
        {
            Transform next = PopNearest(_pendingSuspicion, player ? player.position : transform.position);
            if (next != null)
            {
                _movingToSuspicionPoint = true;
                if (AgentIsValid())
                    agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(next.position);
                return;
            }
        }

        RaiseAnimState(AnimState.Idle);
    }

    protected virtual void TickDanger(bool seesPlayer, Vector3 seenPos)
    {
        agent.speed = chaseSpeed;

        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            lastSeenTime = Time.time;

            float stopDist = _iattackStrategy?.StopDistance ?? 1.5f;
            agent.stoppingDistance = stopDist;

            float dist = Vector3.Distance(transform.position, seenPos);

            // Perseguir si estoy lejos
            if (dist > agent.stoppingDistance + 0.05f)
            {
                if (AgentIsValid())
                    agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(player.position);
                RaiseBool(Shoot_Hash, false);
            }
            else
            {
                // Estoy en rango -> parar, mirar y atacar
                bool standing = agent.velocity.sqrMagnitude < 0.1f;
                RaiseBool(AimBool_Hash, true);

                if (AgentIsValid())
                    agent.isStopped = true;

                agent.ResetPath();
                FaceTowards(seenPos);

                if (_iattackStrategy != null && _iattackStrategy.CanAttack(player, seenPos))
                {
                    _iattackStrategy.Attack(player, seenPos);
                    RaiseBool(Shoot_Hash, true);
                }
            }
            return;
        }

        // No lo veo ahora
        //if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        //{
        //    RaiseAnimState(AnimState.Idle);
        //}
        //else
        //{
        //    RaiseAnimState(AnimState.Idle);
        //}

        // Grace period: todavía recuerdo dónde lo vi
        if (Time.time - lastSeenTime < lostSightGrace)
        {
            agent.stoppingDistance = 0f;

            if (AgentIsValid())
                agent.isStopped = false;

            Vector3 dest = _lastKnownPos;

            // "olfato / cerca"
            if (TryNearDetectPlayer(out var sensed) && IsReachable(sensed, out var navSensed))
            {
                _lastKnownPos = navSensed;
                dest = navSensed;
            }

            if (AgentIsValid())
                agent.SetDestination(dest);

            FaceTowards(dest);
            RaiseBool(Shoot_Hash, false);
            return;
        }

        // Se perdió del todo -> ir a la última posición conocida y luego pasar a Suspicious
        agent.stoppingDistance = 0f;

        if (AgentIsValid())
            agent.isStopped = false;

        if (AgentIsValid())
            agent.SetDestination(_lastKnownPos);

        RaiseBool(Shoot_Hash, false);

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            SetState(EnemyState.Suspicious);
        }
    }

    protected virtual void SetState(EnemyState next)
    {
        if (_state == next) return;

        _state = next;

        switch (_state)
        {
            case EnemyState.Patrolling:
                agent.stoppingDistance = 0f;
                if (AgentIsValid())
                    agent.isStopped = false;
                agent.updateRotation = true;
                break;

            case EnemyState.Suspicious:
                agent.stoppingDistance = 0.1f;
                _scanActive = false;
                _scanTimer = 0f;
                _movingToSuspicionPoint = true;
                _pendingSuspicion = new List<Transform>(_suspiciousList);

                if (AgentIsValid())
                    agent.isStopped = false;
                agent.updateRotation = true;
                RaiseAnimState(AnimState.Suspicious);
                break;

            case EnemyState.Danger:
                _scanActive = false;
                _movingToSuspicionPoint = false;

                if (AgentIsValid())
                    agent.isStopped = false;
                agent.updateRotation = true;
                break;
        }

        OnStateChange?.Invoke(_state);
    }

    public void Investigate(Vector3 worldPoint)
    {
        _lastKnownPos = worldPoint;
        _pendingSuspicion = new List<Transform>();
        _movingToSuspicionPoint = true;

        if (AgentIsValid())
            agent.isStopped = false;

        agent.updateRotation = true;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(worldPoint);

        if (_state != EnemyState.Suspicious)
            SetState(EnemyState.Suspicious);
    }


    // ============================
    //          Perception
    // ============================

    public bool TrySeePlayer(out Vector3 seenPos)
    {
        seenPos = Vector3.zero;
        if (!player) return false;

        Vector3 origin = GetEyesTransformPos();          // ojos del enemigo
        Vector3 target = GetTargetAimPoint(player);      // ojos del jugador

        Vector3 dir = target - origin;
        float dist = dir.magnitude;
        if (dist > visionRange) return false;

        dir = dir.normalized;

        // armo máscara pero excluyendo la capa del Player
        int mask = obstacleMask & ~(1 << player.gameObject.layer);

        float angle = Vector3.Angle(GetForward(), dir);
        if (angle > visionAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist + 0.1f, mask, QueryTriggerInteraction.Ignore))
            return false;

        seenPos = target;
        return true;
    }

    private bool TryNearDetectPlayer(out Vector3 sensedPos)
    {
        sensedPos = Vector3.zero;
        if (_state != EnemyState.Danger) return false;
        if (!player) return false;

        Vector3 origin = nearDetect.position;
        Vector3 target = GetTargetAimPoint(player);

        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist > proximityRadius) return false;

        sensedPos = target;
        return true;
    }


    // ============================
    //          Patrol
    // ============================

    private void AdvancePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (_patrolIndex >= patrolPoints.Length)
            _patrolIndex = 0;

        if (patrolPoints[_patrolIndex] == null) return;

        agent.SetDestination(patrolPoints[_patrolIndex].position);
        _patrolIndex++;
    }


    // ============================
    //        Target Helpers
    // ============================

    private Vector3 GetTargetAimPoint(Transform t)
    {
        if (t == null) return Vector3.zero;

        float h = 1.8f;
        var aim = t.GetComponent<IHeightProvider>();
        if (aim != null)
            h = aim.GetEyeHeight();

        return t.position + Vector3.up * h;
    }

    private Vector3 GetEyesTransformPos()
    {
        if (eyes != null)
            return eyes.position;

        return transform.position + Vector3.up * eyesHeight;
    }

    private Vector3 GetForward()
    {
        return (eyes ? eyes.forward : transform.forward).normalized;
    }


    // ============================
    //   Helpers & Misc Utilities
    // ============================

    bool AgentIsValid()
    {
        if (agent == null) return false;
        if (!agent.isActiveAndEnabled) return false;
        if (!agent.isOnNavMesh) return false;
        return true;
    }

    protected void FaceTowards(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir = new Vector3(dir.x, 0f, dir.z);
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            turnSpeed * Time.deltaTime
        );
    }

    bool IsReachable(Vector3 point, out Vector3 navPos)
    {
        navPos = point;

        if (!NavMesh.SamplePosition(point, out var hit, 1.5f, NavMesh.AllAreas))
            return false;

        navPos = hit.position;

        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(transform.position, navPos, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }

    private Transform PopNearest(List<Transform> list, Vector3 from)
    {
        if (list == null || list.Count == 0) return null;

        if (_nearestPointSuspicious)
        {
            int bestIdx = -1;
            float best = float.MaxValue;

            for (int i = 0; i < list.Count; i++)
            {
                float d = (list[i].position - from).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestIdx = i;
                }
            }

            if (bestIdx == -1) return null;

            Transform nearest = list[bestIdx];
            list.RemoveAt(bestIdx);
            return nearest;
        }
        else
        {
            Transform first = list[0];
            list.RemoveAt(0);
            return first;
        }
    }


    // ============================
    //        Suspicion Scan
    // ============================

    private void BeginScan()
    {
        if (visionPivot)
            visionPivot.localRotation = _pivotBaseLocalRot;

        _scanActive = true;
        _scanTimer = 0f;

        RaiseAnimState(AnimState.Scan);

        if (AgentIsValid())
            agent.isStopped = true;

        // Lo paro para que rote SOLO con visionPivot
        agent.updateRotation = false;
    }

    private void EndScan()
    {
        _scanActive = false;
        agent.updateRotation = true;

        if (AgentIsValid())
            agent.isStopped = false;
    }

    private bool UpdateScan()
    {
        _scanTimer += Time.deltaTime;

        // ángulo oscilante izquierda-derecha
        float angle = Mathf.Sin(_scanTimer * 2f * Mathf.PI * scanOscillationsPerSecond)
                    * scanYawAmplitude;

        if (visionPivot != null)
        {
            visionPivot.localRotation =
                _pivotBaseLocalRot * Quaternion.Euler(0f, angle, 0f);
        }

        // terminó de escanear?
        if (_scanTimer >= scanDuration)
        {
            _scanActive = false;

            if (AgentIsValid())
                agent.isStopped = false;

            agent.updateRotation = true;

            if (visionPivot)
                visionPivot.localRotation = _pivotBaseLocalRot;

            return true;
        }

        return false;
    }


    // ============================
    //       Combat / Damage
    // ============================

    private void HandleDamaged_EnterDanger()
    {
        if (_enemyHealth == null || _enemyHealth.IsDead) return;
        StopCoroutine(nameof(DelayedEnterDanger));
        StartCoroutine(DelayedEnterDanger());
    }

    private IEnumerator DelayedEnterDanger()
    {
        if (_enemyHealth == null || _enemyHealth.IsDead) yield break;
        yield return new WaitForSeconds(0.3f);

        _lastKnownPos = player.position;
        SetState(EnemyState.Danger);
    }

    private void HandleDeath()
    {
        enabled = false;

        if (agent && agent.isActiveAndEnabled)
            agent.enabled = false;
    }


    // ============================
    //         Animation API
    // ============================

    protected virtual void RaiseAnimState(AnimState st) => OnAnimState?.Invoke(st);
    protected virtual void RaiseTrigger(int hash) => OnAnimTrigger?.Invoke(hash);
    protected virtual void RaiseBool(int paramHash, bool value) => OnAnimBool?.Invoke(paramHash, value);


    // ============================
    //           Events
    // ============================

    private void OnNoiseHeard(Vector3 pos, float radius)
    {
        if ((pos - transform.position).sqrMagnitude <= radius * radius)
        {
            if (_state != EnemyState.Danger)
                Investigate(pos);
        }
    }


    // ============================
    //           Gizmos
    // ============================
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetEyesTransformPos(), visionRange);

        Vector3 origin = GetEyesTransformPos();
        Vector3 fwd = GetForward();
        float half = visionAngle * 0.5f;
        Quaternion left = Quaternion.AngleAxis(-half, Vector3.up);
        Quaternion right = Quaternion.AngleAxis(half, Vector3.up);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, left * fwd * visionRange);
        Gizmos.DrawRay(origin, right * fwd * visionRange);

        if (_lastKnownPos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_lastKnownPos, 0.3f);
            Gizmos.DrawWireSphere(_lastKnownPos, 5f);
        }

        if (nearDetect != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(nearDetect.position, proximityRadius);
        }
    }
}
