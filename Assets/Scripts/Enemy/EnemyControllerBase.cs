using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyControllerBase : MonoBehaviour , IVisionProvider
{
    // ============================
    //            Types
    // ============================
    public enum EnemyState { Patrolling, Suspicious, Danger }

    // ============================
    //        Inspector Fields
    // ============================
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;         // Opcional
    [SerializeField] private Transform eyes;            // Punto de visión (si null usa transform)
    [SerializeField] private Transform player;          // *** ÚNICO objetivo ***

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointTolerance = 0.6f;
    [SerializeField] private bool loopPatrol = true;
    private int _patrolIndex;

    [Header("Suspicion (Simple)")]
    [SerializeField] private Transform[] suspicionPoints;
    [SerializeField] private float scanDuration = 5f;
    [SerializeField] private float scanYawAmplitude = 70f;
    [SerializeField] private float scanOscillationsPerSecond = 0.2f;

    [Header("Perception")]
    [SerializeField] private LayerMask obstacleMask;    // por defecto todo
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float eyesHeight = 1.7f;

    [Header("Suspicion/Search")]
    [SerializeField] private float lostSightGrace = 2f;
    [SerializeField] private float searchRadius = 5f;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 360f;

    [Header("Proximity / Awareness")]
    [SerializeField] private float proximityRadius = 5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    // ============================
    //        Runtime State
    // ============================
    private EnemyState _state = EnemyState.Patrolling;

    private Vector3 _lastKnownPos;      // Última posición conocida/ruido
    private float _lostSightTimer;

    private bool _scanActive;
    private float _scanTimer;
    private float _scanBaseYaw;
    private bool _movingToSuspicionPoint;

    private List<Transform> _suspiciousList;

    private AttackBase _attackBase;

    // ============================
    //      Unity Lifecycle
    // ============================
    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;

        // Estado inicial
        SetState(EnemyState.Patrolling);

        // Primer destino de patrulla
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[_patrolIndex].position);

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
            obstacleMask &= ~(1 << playerLayer);

        if (!eyes)
            eyes = transform.Find("Eyes");

        _attackBase = GetComponent<AttackBase>();
    }

    private void Update()
    {
        // Percepción: chequea sólo al jugador
        bool seesPlayer = TrySeePlayer(out Vector3 seenPos);

        // Si no lo veo por cono, pruebo detección cercana
        if (!seesPlayer && TryNearDetectPlayer(out Vector3 sensedPos))
        {
            seesPlayer = true;
            seenPos = sensedPos; // tratar como visto/detectado
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

        UpdateAnimator();
    }

    // ============================
    //        State Machine
    // ============================

    private void TickPatrolling(bool seesPlayer, Vector3 seenPos)
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

        // FaceForwardSlowly();   // Usalo si queres que patrulle girando en 360º
    }

    private void TickSuspicious(bool seesPlayer, Vector3 seenPos)
    {
        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            SetState(EnemyState.Danger);
            return;
        }

        // Fase 1: ir al punto (suspicionPoint más cercano o LKP)
        if (_movingToSuspicionPoint)
        {
            agent.speed = patrolSpeed * 1.3f;

            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                // Llegamos: arrancar escaneo
                _movingToSuspicionPoint = false;
                _scanActive = true;
                _scanTimer = 0f;
                agent.isStopped = true;
                agent.updateRotation = false;
                _scanBaseYaw = transform.eulerAngles.y; // referencia para oscilar
            }
            return;
        }

        // Fase 2: escaneo in-situ (izq/der) por un tiempo y volver a patrullar
        if (_scanActive)
        {
            _scanTimer += Time.deltaTime;

            // Oscilación senoidal: base ± amplitud
            float angle = _scanBaseYaw + Mathf.Sin(_scanTimer * 2f * Mathf.PI * scanOscillationsPerSecond) * scanYawAmplitude;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            if (_scanTimer >= scanDuration)
            {
                _scanActive = false;             // <- salgo del modo escaneo
                agent.isStopped = false;
                agent.updateRotation = true;
                return;
            }
            return;
        }

        // Elegir siguiente punto de sospecha (si hay)
        if (!_scanActive && _suspiciousList != null && _suspiciousList.Count > 0)
        {
            Transform next = PopNearest(_suspiciousList, transform.position);
            if (next != null)
            {
                _movingToSuspicionPoint = true;

                agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(next.position);
                return;
            }
        }
        else
        {
            // No hay más puntos: volver a patrullar
            agent.isStopped = false;
            agent.updateRotation = true;
            _scanActive = false;
            SetState(EnemyState.Patrolling);
        }
    }

    private void TickDanger(bool seesPlayer, Vector3 seenPos)
    {
        agent.speed = chaseSpeed;

        if (seesPlayer)
        {
            _lastKnownPos = seenPos;
            _lostSightTimer = 0f;

            // El stoppingDistance depende del ataque (ej: melee a 1.5m)
            agent.stoppingDistance = Mathf.Max(_attackBase.StopDistance, agent.radius + 0.1f);

            // ✅ Solo perseguir si estoy fuera de stoppingDistance
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > agent.stoppingDistance + 0.05f)  // margen pequeño para evitar vibración
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.ResetPath(); // se queda quieto
                _attackBase?.Attack(player, seenPos);
            }
        }
        else
        {
            // Perdió de vista al jugador
            _lostSightTimer += Time.deltaTime;
            if (_lostSightTimer >= lostSightGrace)
            {
                SetState(EnemyState.Suspicious);
                return;
            }

            // Ir al último punto conocido
            agent.SetDestination(_lastKnownPos);
        }
    }

    private void SetState(EnemyState next)
    {
        _state = next;

        switch (_state)
        {
            case EnemyState.Patrolling:
                agent.stoppingDistance = 0f;
                _lostSightTimer = 0f;

                if (patrolPoints != null && patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[_patrolIndex].position);
                break;

            case EnemyState.Suspicious:
                {
                    agent.stoppingDistance = 0.1f;

                    _scanActive = false;
                    _scanTimer = 0f;

                    // Construye la lista de puntos válidos
                    _suspiciousList = null;

                    if (suspicionPoints != null && suspicionPoints.Length > 0)
                    {
                        _suspiciousList = new List<Transform>(suspicionPoints.Length);
                        foreach (var t in suspicionPoints)
                            if (t && t.gameObject.activeInHierarchy)
                                _suspiciousList.Add(t);
                    }

                    // **PRIORIDAD**: si hay LKP, ir primero ahí. Si no hay LKP, recién ahí elegir el nearest.
                    Vector3 targetPos;
                    if (_lastKnownPos != Vector3.zero)
                    {
                        targetPos = _lastKnownPos; // <- primero LKP
                    }
                    else
                    {
                        Transform first = (_suspiciousList != null) ? PopNearest(_suspiciousList, transform.position) : null;
                        targetPos = (first != null) ? first.position : transform.position;
                    }

                    _movingToSuspicionPoint = true;
                    agent.isStopped = false;
                    agent.updateRotation = true;
                    agent.SetDestination(targetPos);
                    break;
                }

            case EnemyState.Danger:

                _lostSightTimer = 0f;
                break;
        }

        if (animator)
        {
            animator.SetBool("IsPatrolling", _state == EnemyState.Patrolling);
            animator.SetBool("IsSuspicious", _state == EnemyState.Suspicious);
            animator.SetBool("IsDanger", _state == EnemyState.Danger);
        }
    }

    // ============================
    //          Perception
    // ============================
    public bool TrySeePlayer(out Vector3 seenPos) // Cono
    {
        seenPos = Vector3.zero;
        if (!player) return false;

        Vector3 origin = GetEyesWorldPos();
        Vector3 target = GetTargetAimPoint(player);
        Vector3 dir = target - origin;

        float dist = dir.magnitude;
        if (dist > visionRange) return false;

        dir /= (dist > 0.0001f ? dist : 1f);

        // Chequeo de cono (ángulo)
        float angle = Vector3.Angle(GetForward(), dir);
        if (angle > visionAngle * 0.5f) return false;

        // LdV mediante raycast
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist + 0.1f, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            // Si pega algo de obstacleMask antes que el player, no lo ve
            // (Como obstacleMask no incluye al player, este raycast es suficiente)
            return false;
        }

        seenPos = target;
        return true;
    }

    private bool TryNearDetectPlayer(out Vector3 sensedPos)  // Esfera alrededor
    {
        sensedPos = Vector3.zero;
        if (!player) return false;

        Vector3 origin = GetEyesWorldPos();                // podés usar transform.position si preferís
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

        _patrolIndex++;
        if (_patrolIndex >= patrolPoints.Length)
            _patrolIndex = loopPatrol ? 0 : patrolPoints.Length - 1;

        agent.SetDestination(patrolPoints[_patrolIndex].position);
    }

    // ============================
    //        Target Helpers
    // ============================
    private Vector3 GetTargetAimPoint(Transform t)
    {
        float h = 1.8f;

        var aim = t.GetComponent<IHeightProvider>(); // Uso la interface para conseguir la posicion de los ojos de mi jugador, y poder agacharme y que no me vea
        if (aim != null) h = aim.GetEyeHeight();

        return t.position + Vector3.up * h;
    }

    private Vector3 GetEyesWorldPos()
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

        // Limpia nulos/inactivos
        for (int i = list.Count - 1; i >= 0; --i)
            if (!list[i] || !list[i].gameObject.activeInHierarchy)
                list.RemoveAt(i);

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
        list.RemoveAt(bestIdx);          // <- lo DESCARTA de pendientes
        return nearest;                  // <- lo devolvemos como destino actual
    }

    private void FaceForwardSlowly()   // Usalo si queres que patrulle girando en 360º
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.Euler(0f, transform.eulerAngles.y + 60f, 0f),
            turnSpeed * 0.05f * Time.deltaTime
        );
    }

    private void FaceTowards(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir = new Vector3(dir.x, 0f, dir.z);   // Ignoro Y para que el agente se mueva solo por el suelo
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    // ============================
    //        Animator Glue
    // ============================
    private void UpdateAnimator()
    {
        if (!animator) return;
        float speed = agent ? agent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed);
    }

    // ============================
    //           Gizmos
    // ============================
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        // Rango de visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetEyesWorldPos(), visionRange);

        // Cono de visión (dos rayos límites)
        Vector3 origin = GetEyesWorldPos();
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
            Gizmos.DrawWireSphere(_lastKnownPos, searchRadius);
        }

        // Radio de proximidad
        Gizmos.DrawWireSphere(GetEyesWorldPos(), proximityRadius);
    }
}
