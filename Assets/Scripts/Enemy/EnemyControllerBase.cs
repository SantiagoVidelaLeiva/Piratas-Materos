using System.Collections.Generic;
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
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointTolerance = 0.6f;
    [SerializeField] private bool loopPatrol = true;
    private int _patrolIndex;

    [Header("Suspicion (Simple)")]
    [SerializeField] private List<Transform> _suspiciousList;
    [SerializeField] private float scanDuration = 5f;
    [SerializeField] private float scanYawAmplitude = 70f;
    [SerializeField] private float scanOscillationsPerSecond = 0.2f;

    [Header("Perception")]
    [SerializeField] private LayerMask obstacleMask = ~0;    // por defecto todo
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float eyesHeight = 1.7f;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] protected float chaseSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Proximity / Awareness")]
    [SerializeField] private float proximityRadius = 5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    // ============================
    //        Runtime State
    // ============================
    private EnemyState _state = EnemyState.Patrolling;
    public EnemyState CurrentState { get { return _state; } } // Como otros scripts ven la misma variable

    public event System.Action<EnemyState> OnStateChange;
    public event System.Action OnEnemyDestroyed;

    protected Vector3 _lastKnownPos;

    private bool _scanActive;
    private float _scanTimer;
    private float _scanBaseYaw;
    private bool _movingToSuspicionPoint;
    private List<Transform> _pendingSuspicion;

    protected IAttackStrategy _iattackStrategy;

    // ============================
    //      Unity Lifecycle
    // ============================
    protected virtual void Awake() // ← antes private
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;

        SetState(EnemyState.Patrolling);

        if (!eyes)
            eyes = transform.Find("Eyes");
        if (!nearDetect)
            nearDetect = transform.Find("NearDetect");

        _iattackStrategy = GetComponent<IAttackStrategy>();

        NoiseSystem.OnNoise += OnNoiseHeard;
    }


    private void OnDestroy()
    {
        NoiseSystem.OnNoise -= OnNoiseHeard;
        OnEnemyDestroyed?.Invoke();
    }

    private void OnNoiseHeard(Vector3 pos, float radius)
    {
        if ((pos - transform.position).sqrMagnitude <= radius * radius)
        {
            // Solo si no ve al player y no está en Danger, o permití override según tu diseño
            if (_state != EnemyState.Danger)
                Investigate(pos);
        }
    }

    private void Update()
    {
        bool seesPlayer = TrySeePlayer(out Vector3 seenPos);

        if (!seesPlayer && TryNearDetectPlayer(out Vector3 sensedPos))
        {
            seesPlayer = true;
            seenPos = sensedPos;
        }

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
    }

    // ============================
    //        State Machine
    // ============================
    protected virtual void TickPatrolling(bool seesPlayer, Vector3 seenPos) // ← antes private
    {
        agent.speed = patrolSpeed;

        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            SetState(EnemyState.Danger);
            return;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
                AdvancePatrol();
        }
    }

    protected virtual void TickSuspicious(bool seesPlayer, Vector3 seenPos)
    {
        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            SetState(EnemyState.Danger);
            return;
        }

        if (_movingToSuspicionPoint)
        {
            agent.speed = patrolSpeed * 1.3f;

            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                _movingToSuspicionPoint = false;
                BeginScan();
            }
            return;
        }

        if (_scanActive)
        {
            bool finished = UpdateScan();
            if (finished)
            {
                // Si tenés puntos pendientes, andá al siguiente
                Transform next = PopNearest(_pendingSuspicion, transform.position);
                if (next != null)
                {
                    _movingToSuspicionPoint = true;
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    agent.SetDestination(next.position);
                }
                else
                {
                    SetState(EnemyState.Patrolling);
                }
            }
            return;
        }

        if (_pendingSuspicion != null && _pendingSuspicion.Count > 0)
        {
            Transform next = PopNearest(_pendingSuspicion, transform.position);
            if (next != null)
            {
                _movingToSuspicionPoint = true;
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(next.position);
                return;
            }
        }

        SetState(EnemyState.Patrolling);
    }

    protected virtual void TickDanger(bool seesPlayer, Vector3 seenPos) 
    {
        agent.speed = chaseSpeed;

        if (seesPlayer)
        {
            _lastKnownPos = seenPos;

            agent.stoppingDistance = _iattackStrategy.StopDistance;

            float dist = Vector3.Distance(transform.position, seenPos);
            if (dist > agent.stoppingDistance + 0.05f)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.ResetPath();
                FaceTowards(seenPos);
                if (_iattackStrategy.CanAttack(player, seenPos))
                {
                    _iattackStrategy.Attack(player, seenPos);
                }
            }
        }
        else
        {
            agent.SetDestination(_lastKnownPos);

            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                SetState(EnemyState.Suspicious);
            }
        }
    }

    protected virtual void SetState(EnemyState next) // ← antes private
    {
        if (_state == next) return;

        _state = next;

        switch (_state)
        {
            case EnemyState.Patrolling:
                agent.stoppingDistance = 0f;
                break;

            case EnemyState.Suspicious:
                {
                    agent.stoppingDistance = 0.1f;

                    _scanActive = false;
                    _scanTimer = 0f;

                    _movingToSuspicionPoint = true;
                    _pendingSuspicion = new List<Transform>(_suspiciousList);
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    break;
                }

            case EnemyState.Danger:
                break;
        }

        OnStateChange?.Invoke(_state);
    }
    public void Investigate(Vector3 worldPoint, float customScanDuration = -1f, float customYawAmp = -1f, float customFreq = -1f)
    {
        // Opcionalmente permitir tuning por evento
        if (customScanDuration > 0f) scanDuration = customScanDuration;
        if (customYawAmp > 0f) scanYawAmplitude = customYawAmp;
        if (customFreq > 0f) scanOscillationsPerSecond = customFreq;

        _lastKnownPos = worldPoint;
        _pendingSuspicion = new List<Transform>(); // vacía para que vaya directo al punto
        _movingToSuspicionPoint = true;

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(worldPoint);

        // Entrá a SUSPICIOUS si no estaba ya
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

        Vector3 origin = GetEyesTransformPos();
        Vector3 target = GetTargetAimPoint(player);
        Vector3 dir = target - origin;

        float dist = dir.magnitude;
        if (dist > visionRange) return false;

        dir = dir.normalized;

        float angle = Vector3.Angle(GetForward(), dir);
        if (angle > visionAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist + 0.1f, obstacleMask))
        {
            return false;
        }

        seenPos = target;
        return true;
    }

    private bool TryNearDetectPlayer(out Vector3 sensedPos)
    {
        sensedPos = Vector3.zero;
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
        if (aim != null) h = aim.GetEyeHeight();

        return t.position + Vector3.up * h;
    }

    private Vector3 GetEyesTransformPos()
    {
        if (eyes != null) return eyes.position;
        return transform.position + Vector3.up * eyesHeight;
    }

    private Vector3 GetForward()
    {
        return (eyes ? eyes.forward : transform.forward).normalized;
    }

    // ============================
    //   Helpers & Misc Utilities
    // ============================
    private Transform PopNearest(List<Transform> list, Vector3 from)
    {
        if (list == null || list.Count == 0) return null;

        int bestIdx = -1;
        float best = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            Transform t = list[i];
            float d = (t.position - from).sqrMagnitude;
            if (d < best) { best = d; bestIdx = i; }
        }

        if (bestIdx == -1) return null;

        Transform nearest = list[bestIdx];
        list.RemoveAt(bestIdx);
        return nearest;
    }
    // EnemyControllerBase.cs (agregar helpers)
    private void BeginScan()
    {
        _scanActive = true;
        _scanTimer = 0f;
        agent.isStopped = true;
        agent.updateRotation = false;
        _scanBaseYaw = transform.eulerAngles.y;
    }

    private bool UpdateScan()
    {
        _scanTimer += Time.deltaTime;

        float angle = _scanBaseYaw + Mathf.Sin(_scanTimer * 2f * Mathf.PI * scanOscillationsPerSecond) * scanYawAmplitude;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        if (_scanTimer >= scanDuration)
        {
            _scanActive = false;
            agent.isStopped = false;
            agent.updateRotation = true;
            return true; // terminó
        }
        return false; // sigue
    }


    protected void FaceTowards(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir = new Vector3(dir.x, 0f, dir.z);
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    // ============================
    //           Gizmos
    // ============================
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        // Rango de visión (desde los ojos)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetEyesTransformPos(), visionRange);

        // Cono de visión
        Vector3 origin = GetEyesTransformPos();
        Vector3 fwd = GetForward();
        float half = visionAngle * 0.5f;
        Quaternion left = Quaternion.AngleAxis(-half, Vector3.up);
        Quaternion right = Quaternion.AngleAxis(half, Vector3.up);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, left * fwd * visionRange);
        Gizmos.DrawRay(origin, right * fwd * visionRange);

        // Último punto conocido
        if (_lastKnownPos != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_lastKnownPos, 0.3f);
            Gizmos.DrawWireSphere(_lastKnownPos, 5f);
        }

        // 🔹 Proximidad (desde nearDetect, no desde los ojos)
        if (nearDetect != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(nearDetect.position, proximityRadius);
        }
    }
}
