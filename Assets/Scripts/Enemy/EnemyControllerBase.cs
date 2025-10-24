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

    [Header("Animations")]
    //[SerializeField] private Animator animator;


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
    private bool _movingToSuspicionPoint;
    private List<Transform> _pendingSuspicion;

    protected IAttackStrategy _iattackStrategy;
    private CharacterHealth health;
    static readonly int Shoot_Hash = Animator.StringToHash("Shoot");
    
    private bool _isScaning;
    int upperBodyLayerIdx;
    Quaternion _pivotBaseLocalRot;
    EnemyAnimator _enemyAnimator;
    // ============================
    //      Unity Lifecycle
    // ============================
    protected virtual void Awake() // ← antes private
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

        NoiseSystem.OnNoise += OnNoiseHeard;
        health = GetComponent<CharacterHealth>();
        if (health != null)
            health.OnDied += HandleDeath;
        else
            Debug.LogError($"[{name}] Falta CharacterHealth");

        //if (!animator)
        //    animator = GetComponentInChildren<Animator>(true);
        if (!_enemyAnimator)
            _enemyAnimator = GetComponentInChildren<EnemyAnimator>(true);
        if (patrolPoints != null && patrolPoints.Length > 0)
            _enemyAnimator.SetState(AnimState.Patrolling);


    }
    void Start()
    {
        //_enemyAnimator.SetState(AnimState.Idle);
        //upperBodyLayerIdx = animator.GetLayerIndex("UpperBody");
        SetState(EnemyState.Patrolling);
    }

    private void OnDestroy()
    {
        NoiseSystem.OnNoise -= OnNoiseHeard;
        OnEnemyDestroyed?.Invoke();
        if (health != null)
            health.OnDied -= HandleDeath;
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

        //if (!seesPlayer && TryNearDetectPlayer(out Vector3 sensedPos)) // Si no lo veo, pero lo veo con la esfera del "olfato"
        //{
        //    seesPlayer = true;
        //    seenPos = sensedPos;
        //}

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
        //bool dead = (health != null) && health.IsDead;
        //UpdateAnimator(agent.velocity,_isScaning, dead);
        //UpdateUpperBodyWeight();

    }
    //void LateUpdate()
    //{
    //    var st = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIdx);
    //    Debug.Log($"[SCAN] layer={upperBodyLayerIdx} state={st.shortNameHash} norm={st.normalizedTime:0.00} loop={st.loop} weight={animator.GetLayerWeight(upperBodyLayerIdx):0.00}");
    //}
    //void UpdateAnimator(Vector3 vel,bool scan, bool dead)
    //{
    //    if (!animator) return;
    //    float raw = new Vector3(vel.x, 0, vel.z).magnitude;     // m/s reales
    //    float norm = Mathf.InverseLerp(0f, chaseSpeed, raw);    // 0..1
    //    animator.SetFloat("Speed01", norm, 0.1f, Time.deltaTime); // <— usa norm
    //    //animator.SetBool("IsScaning", scan);
    //    animator.SetBool("IsDead", dead);

    //    //// Si estás usando un rig de apuntado o mira
    //    //float targetAim = (seesPlayer ? 1f : 0f);
    //    //_aimWeight = Mathf.MoveTowards(_aimWeight, targetAim, Time.deltaTime * 4f);
    //    //animator.SetFloat("AimWeight", _aimWeight);

    //    if (dead)
    //    {
    //        int style = (norm > 0.5f) ? 1 : 0; // 0=quieto, 1=corriendo
    //        animator.SetInteger("DeathStyle", style);
    //    }
    //}
    private void HandleDeath()
    {
        //animator.SetBool("IsDead", true);
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
            // si no hay puntos, quedarse quieto
            agent.isStopped = true;
            _enemyAnimator.SetState(AnimState.Idle);
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
            if (dist > agent.stoppingDistance + 0.05f)                  // Si la distancia es mayor al rango del ataque de la interface, seguir persiguiendo
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {
                FaceTowards(seenPos);                // Miro hacia adelante donde esta mi jugador
                if (_iattackStrategy != null && _iattackStrategy.CanAttack(player, seenPos))     // Llamo a la interface si puedo atacar
                {
                    agent.isStopped = true;
                    agent.ResetPath();                   // Cancelo la ruta del agente
                    _iattackStrategy.Attack(player, seenPos);  // Ataco
                    _enemyAnimator.SetTrigger(Shoot_Hash);
                }
                else
                {
                    // micro ajuste para no quedar clavado si aún no puede atacar
                    agent.isStopped = false;
                    agent.SetDestination(seenPos);
                }
            }
            return;
        }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            _enemyAnimator.SetState(AnimState.Idle);
        }
        else
        {
            _enemyAnimator.SetState(AnimState.Danger);
        }
        if (Time.time - lastSeenTime < lostSightGrace)
        {
            agent.stoppingDistance = 0f;

            agent.isStopped = false;

            Vector3 dest = _lastKnownPos;
            if (TryNearDetectPlayer(out var sensed) && IsReachable(sensed, out var navSensed))
            {
                _lastKnownPos = navSensed;
                dest = navSensed;
            }
            agent.SetDestination(dest);
            FaceTowards(dest);                          
            return;
        }

        agent.stoppingDistance = 0f;
        agent.isStopped = false;
        agent.SetDestination(_lastKnownPos);                 // Linea de codigo que mueve al agente al LKP

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
                agent.isStopped = false;
                agent.updateRotation = true;
                _enemyAnimator.SetState(AnimState.Patrolling);
                break;

            case EnemyState.Suspicious:
                agent.stoppingDistance = 0.1f;
                _scanActive = false;
                _scanTimer = 0f;
                _movingToSuspicionPoint = true;
                _pendingSuspicion = new List<Transform>(_suspiciousList);
                agent.isStopped = false;
                agent.updateRotation = true;
                _enemyAnimator.SetState(AnimState.Suspicious);
                break;

            case EnemyState.Danger:
                _scanActive = false;
                _movingToSuspicionPoint = false;
                agent.isStopped = false;
                agent.updateRotation = true;
                _enemyAnimator.SetState(AnimState.Danger);
                //agent.autoBraking = false;
                break;
        }

        OnStateChange?.Invoke(_state);
    }
    public void Investigate(Vector3 worldPoint)
    {
        _lastKnownPos = worldPoint;
        _pendingSuspicion = new List<Transform>(); // Vacia para que vaya directo al punto
        _movingToSuspicionPoint = true;

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
        _enemyAnimator.SetState(AnimState.Scan);
        agent.isStopped = true;
        agent.updateRotation = false;  // Para que cuando llegue al LKP pueda scanear con mi codigo y no se mueva el agente solo
    }
    private void EndScan()
    {
        _scanActive = false;
        _enemyAnimator.SetState(AnimState.Suspicious);
        agent.updateRotation = true;
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
