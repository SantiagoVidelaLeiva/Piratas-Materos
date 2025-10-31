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
    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointTolerance = 2f;
    private int _patrolIndex;

    [Header("Suspicion (Simple)")]
    [SerializeField] private List<Transform> _suspiciousList;
    [SerializeField] private float scanDuration = 5f;
    [SerializeField] private float scanYawAmplitude = 70f;
    [SerializeField] private float scanOscillationsPerSecond = 0.2f;
    [SerializeField] private bool _nearestPointSuspicious = true;

    [Header("Perception")]
    [SerializeField] private LayerMask obstacleMask = ~0;    // por defecto todo
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float eyesHeight = 1.7f;
    private float lastSeenTime;
    private float lostSightGrace = 4;
    [SerializeField] private Transform visionPivot;

    [Header("Speeds")]
    private float patrolSpeed = 2.0f;
    private float suspiciousSpeed = 3.0f;
    protected float chaseSpeed = 4.5f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Proximity / Awareness")]
    [SerializeField] private float proximityRadius = 5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;



    // ============================
    //        Runtime State
    // ============================
    private EnemyState _state = EnemyState.Patrolling;
    public EnemyState CurrentState { get { return _state; } }

    public event Action<EnemyState> OnStateChange;
    public event Action OnEnemyDestroyed;
    public event Action<float> OnSpeed01Changed;
    public event Action<AnimState> OnAnimState;
    public event Action<int> OnAnimTrigger;
    public event Action<int, bool> OnAnimBool;
    protected Vector3 _lastKnownPos;

    private bool _scanActive;
    private float _scanTimer;
    private bool _movingToSuspicionPoint;
    private List<Transform> _pendingSuspicion;

    protected IAttackStrategy _iattackStrategy;
    private CharacterHealth health;
    static readonly int Shoot_Hash = Animator.StringToHash("ShootBool");
    static readonly int AimBool_Hash = Animator.StringToHash("IsAiming");

    private bool _isScaning;
    int upperBodyLayerIdx;
    Quaternion _pivotBaseLocalRot;
    EnemyAnimator _enemyAnimator;
    CharacterHealth _enemyHealth;
    const float moveShootSpeedMax = 0.05f;
    float _lastSpeed01;
    float _smoothNorm;
    [SerializeField] float aimTurnSpeed = 6f;

    // ============================
    //      Unity Lifecycle
    // ============================
    protected virtual void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;

        if (patrolPoints == null)
            patrolPoints = new Transform[0];

        if (!eyes)
            eyes = transform.Find("VisionPivot/Eyes");
        if (!nearDetect)
            nearDetect = transform.Find("VisionPivot/NearDetect");
        if (!visionPivot)
            visionPivot = transform.Find("VisionPivot");
        if (visionPivot) _pivotBaseLocalRot = visionPivot.localRotation;
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
    protected virtual void Start()
    {
        SetState(EnemyState.Patrolling);
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

    private void OnNoiseHeard(Vector3 pos, float radius)
    {
        if ((pos - transform.position).sqrMagnitude <= radius * radius)
        {
            if (_state != EnemyState.Danger)
                Investigate(pos);
        }
    }

    private void Update()
    {
        bool seesPlayer = TrySeePlayer(out Vector3 seenPos);// Si veo al jugador, consigo su posicion de la cabeza y el bool verdadero

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
;
        float raw = new Vector3(agent.velocity.x, 0f, agent.velocity.z).magnitude; // m/s
        float norm = Mathf.InverseLerp(0f, chaseSpeed, raw); // 0..1 usando tu chaseSpeed como tope
        _smoothNorm = Mathf.Lerp(_smoothNorm, norm, Time.deltaTime * 5f);
        _enemyAnimator.SetLayerWeight(_smoothNorm < 0.7f ? 1f : 0f);
        if (Mathf.Abs(norm - _lastSpeed01) > 0.01f) // evita spam
        {
            _lastSpeed01 = norm;
            OnSpeed01Changed?.Invoke(norm);
        }

    }

    private void HandleDeath()
    {
        enabled = false;
        if (agent && agent.isActiveAndEnabled)
            agent.enabled = false;
    }
    // ============================
    //        State Machine
    // ============================
    protected virtual void TickPatrolling(bool seesPlayer, Vector3 seenPos)  // Patrulla
    {
        agent.speed = patrolSpeed; // Ajusto velocidad del agente

        if (seesPlayer) // Si veo al jugador
        {
            _lastKnownPos = seenPos; // Guardo LKP la ultima posicion vista de mi jugador
            SetState(EnemyState.Danger);
            return;
        }
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            if (AgentIsValid())
                agent.isStopped = true;
            return;
        }
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)// Waypoint dice si llegue a X metros, considerar que ya llego para evitar vibraciones
            {
                AdvancePatrol();
            }
            // Si puse por inspector puntos de patrulla los recorre
        }
    }

    protected virtual void TickSuspicious(bool seesPlayer, Vector3 seenPos) // Sospecha
    {
        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            SetState(EnemyState.Danger);
            return;
        }

        if (_movingToSuspicionPoint) // Es true cuando se setea en SetState
        {
            agent.speed = suspiciousSpeed;

            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                _movingToSuspicionPoint = false;
                BeginScan();
            }
            return;
        }

        if (_scanActive)   //Llego a LKP ultima posicion conocida y scanea
        {
            bool finished = UpdateScan();
            if (finished)
            {
                Transform next = PopNearest(_pendingSuspicion, transform.position);
                if (next != null)
                {
                    _movingToSuspicionPoint = true;
                    if (AgentIsValid())
                        agent.isStopped = false;
                    agent.updateRotation = true;
                    EndScan();
                    agent.SetDestination(next.position);
                }
                else
                {
                    EndScan();
                    SetState(EnemyState.Patrolling);
                }
            }
            return;
        }

        if (_pendingSuspicion != null && _pendingSuspicion.Count > 0) // Si tengo puntos de sospecha puestos en el inspector , los recorre.
        {
            Transform next = PopNearest(_pendingSuspicion, player ? player.position : transform.position); // Consigo el punto mas cercano despues de haber perdido al jugador
            if (next != null)
            {
                _movingToSuspicionPoint = true;  // Sube arriba y scanea devuelta en cada punto
                if (AgentIsValid())
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
            lastSeenTime = Time.time;


            float stopDist = _iattackStrategy?.StopDistance ?? 1.5f;
            agent.stoppingDistance = stopDist; // Detiene al agente segun el rango de cada ataque usado en la interface, si da null por CD es 1.5f

            float dist = Vector3.Distance(transform.position, seenPos); //Consigo la distancia en metros desde el enemigo al jugador
            bool isUpperFiring = _enemyAnimator != null && _enemyAnimator.IsUpperFiring();
            if (dist > agent.stoppingDistance + 0.05f && !isUpperFiring)                  // Si la distancia es mayor al rango del ataque de la interface, seguir persiguiendo
            {
                if (AgentIsValid())
                    agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(player.position);
                RaiseBool(Shoot_Hash, false);
            }
            else
            {

                bool standing = agent.velocity.sqrMagnitude < 0.1f;
                RaiseBool(AimBool_Hash, true);
                if (AgentIsValid())
                    agent.isStopped = true;
                agent.ResetPath();                   // Cancelo la ruta del agente
                FaceTowards(seenPos);                // Miro hacia adelante donde esta mi jugador
                if (_iattackStrategy != null && _iattackStrategy.CanAttack(player, seenPos))     // Llamo a la interface si puedo atacar
                {
                    _iattackStrategy.Attack(player, seenPos);  // Ataco
                    RaiseBool(Shoot_Hash,true);
                }
            }
            return;
        }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            RaiseAnimState(AnimState.Idle);
        }
        else
        {
            RaiseAnimState(AnimState.Idle);
        }
        if (Time.time - lastSeenTime < lostSightGrace)
        {
            agent.stoppingDistance = 0f;

            if (AgentIsValid())
                agent.isStopped = false;

            Vector3 dest = _lastKnownPos;
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

        agent.stoppingDistance = 0f;
        if (AgentIsValid())
            agent.isStopped = false;

        if (AgentIsValid())
            agent.SetDestination(_lastKnownPos);                 // Linea de codigo que mueve al agente al LKP
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
                RaiseAnimState(AnimState.Patrolling);
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
                //RaiseAnimState(AnimState.Danger);
                break;
        }

        OnStateChange?.Invoke(_state);
    }
    public void Investigate(Vector3 worldPoint)
    {
        _lastKnownPos = worldPoint;
        _pendingSuspicion = new List<Transform>(); // Vacia para que vaya directo al punto
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
    public bool TrySeePlayer(out Vector3 seenPos)  // Raycast en forma de "cono" que devuelve un bool y el vector3 de la cabeza del jugador
    {
        seenPos = Vector3.zero;
        if (!player) return false;

        Vector3 origin = GetEyesTransformPos(); // Posicion de los ojos del enemigo
        Vector3 target = GetTargetAimPoint(player); // Posicion de los ojos del jugador
        Vector3 dir = target - origin;  // Restar 2 vectores3 te da una direccion
        float dist = dir.magnitude; // Longitud, consigo distancia
        if (dist > visionRange) return false;

        dir = dir.normalized;


        int mask = obstacleMask & ~(1 << player.gameObject.layer); // Excluir la capa del Player del mask de obstaculos

        float angle = Vector3.Angle(GetForward(), dir);
        if (angle > visionAngle * 0.5f) return false;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist + 0.1f, mask, QueryTriggerInteraction.Ignore))
            return false;

        seenPos = target;
        return true;
    }


    private bool TryNearDetectPlayer(out Vector3 sensedPos) // Olfato, esfera al rededor del enemigo
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
    private void AdvancePatrol()  // Patrulla si tengo puntos puestos en el inspector
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
    private Vector3 GetTargetAimPoint(Transform t)  // Devuelve la posicion de los ojos del jugador
    {
        if (t == null) return Vector3.zero;
        float h = 1.8f;

        var aim = t.GetComponent<IHeightProvider>();
        if (aim != null) h = aim.GetEyeHeight();

        return t.position + Vector3.up * h;
    }

    private Vector3 GetEyesTransformPos() // Devuelve la posicion de los ojos del enemigo
    {
        if (eyes != null) return eyes.position;
        return transform.position + Vector3.up * eyesHeight;
    }

    private Vector3 GetForward() // Devuelve un vector de donde esta mirando el enemigo
    {
        return (eyes ? eyes.forward : transform.forward).normalized;
    }

    // ============================
    //   Helpers & Misc Utilities
    // ============================


    bool IsReachable(Vector3 point, out Vector3 navPos)
    {
        navPos = point;

        // 1) Proyectar a NavMesh (evita puntos fuera de la malla)
        if (!NavMesh.SamplePosition(point, out var hit, 1.5f, NavMesh.AllAreas))
            return false;

        navPos = hit.position;

        // 2) Calcular path completo
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(transform.position, navPos, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }
    private Transform PopNearest(List<Transform> list, Vector3 from) // Recorre la lista de los puntos de sospecha que pongo en el inspector y devuelve el mas cercano
    {
        if (list == null || list.Count == 0) return null;

        if (_nearestPointSuspicious)
        {
            int bestIdx = -1;
            float best = float.MaxValue;

            for (int i = 0; i < list.Count; i++)
            {
                float d = (list[i].position - from).sqrMagnitude;
                if (d < best) { best = d; bestIdx = i; }
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
    private void BeginScan()
    {
        if (visionPivot) visionPivot.localRotation = _pivotBaseLocalRot;
        _scanActive = true;
        _scanTimer = 0f;
        RaiseAnimState(AnimState.Scan);
        if (AgentIsValid())
            agent.isStopped = true;
        agent.updateRotation = false;  // Para que cuando llegue al LKP pueda scanear con mi codigo y no se mueva el agente solo
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

        float angle = Mathf.Sin(_scanTimer * 2f * Mathf.PI * scanOscillationsPerSecond) * scanYawAmplitude; // Calculo de funcion seno, que genera un angulo que va de un lado al otro alrededor de scanBaseYaw
        if (visionPivot != null)
        {
            visionPivot.localRotation = _pivotBaseLocalRot * Quaternion.Euler(0f, angle, 0f);
        }

        if (_scanTimer >= scanDuration)
        {
            _scanActive = false;
            if (AgentIsValid())
                agent.isStopped = false;
            agent.updateRotation = true;
            if (visionPivot) visionPivot.localRotation = _pivotBaseLocalRot;
            return true;
        }
        return false;
    }

    protected void FaceTowards(Vector3 targetPos) // Gira al enemigo para mirar al jugador de frente
    {
        Vector3 dir = targetPos - transform.position;
        dir = new Vector3(dir.x, 0f, dir.z);
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }
    bool AgentIsValid()
    {
        if (agent == null) return false;
        if (!agent.isActiveAndEnabled) return false;
        if (!agent.isOnNavMesh) return false;

        return true;
    }
    protected virtual void RaiseAnimState(AnimState st) => OnAnimState?.Invoke(st);

    protected virtual void RaiseTrigger(int hash) => OnAnimTrigger?.Invoke(hash);
    protected virtual void RaiseBool(int paramHash, bool value) => OnAnimBool?.Invoke(paramHash, value);

    private void HandleDamaged_EnterDanger()
    {
        if (_enemyHealth == null || _enemyHealth.IsDead) return;
        StopCoroutine(nameof(DelayedEnterDanger)); // por si llega otro daño antes
        StartCoroutine(DelayedEnterDanger());
    }
    private IEnumerator DelayedEnterDanger()
    {
        if (_enemyHealth == null || _enemyHealth.IsDead) yield break;
        yield return new WaitForSeconds(0.3f);
        _lastKnownPos = player.position;
        SetState(EnemyState.Danger);
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
