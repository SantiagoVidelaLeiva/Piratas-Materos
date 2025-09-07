using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // ─────────────────────────── Configuración / Referencias ───────────────────────────
    [Header("Facing")]
    [SerializeField] CameraOrbit _camOrbit;                 
    [SerializeField] float _faceLerp = 8f;                  // Suaviza el movimiento
    [SerializeField] Transform _target;

    [Header("Animación")]
    [SerializeField] Animator _anim;              
    [SerializeField] bool _rotateToMoveDir = true;

    [Header("Visual (offset)")]
    [SerializeField] Transform _visual;
    [SerializeField] float _visualYOffset = -0.04f; // ajustar visual para que los pies toquen el piso

    // ───────────────────────────────── Movimiento ──────────────────────────────────────
    [Header("Movimiento")]
    CharacterController _cc;
    [SerializeField] float _walkSpeed = 4.5f;
    [SerializeField] float _crouchSpeed = 1.2f;
    [SerializeField] float _runSpeed = 6.5f;
    [SerializeField] float _gravity = -20f;
    [SerializeField] float _jumpHeight = 1.4f;
    Vector3 _velocity;
    Vector3 _lastGroundWorld;
    [SerializeField] bool _isFalling;

    [Header("Inputs")]
    KeyCode _runKey = KeyCode.LeftShift;

    [Header("Agacharse")]
    KeyCode _crouchKey = KeyCode.LeftControl;
    [SerializeField] float _standingHeight = 1.8f;
    [SerializeField] float _crouchingHeight = 1.2f;

    // ──────────────────────────────── Estado runtime ───────────────────────────────────
    enum MovementState { Walk, Run, Crouch }
    MovementState _state;
    bool _isCrouching;
    bool _isRunning;
    float _lastHorizontalSpeed;



    // ─────────────────────────────────── Lifecycle ─────────────────────────────────────
    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _cc.height = _standingHeight;
        _cc.center = new Vector3(0, _standingHeight / 2f, 0);

        if (!_anim) _anim = GetComponentInChildren<Animator>();
        if (_anim) _anim.applyRootMotion = false;

        if (!_visual && _anim) _visual = _anim.transform;   
        if (_visual) _visual.localPosition = new Vector3(0f, _visualYOffset, 0f);

    }

    void Update()
    {
        UpdateState();
        Crouch();
        Jump();
        Run();
        ApplyGravity(); 
        Move();


    }

    void LateUpdate()
    {
        UpdateAnim();
    }

    // ──────────────────────────────────── Lógica ───────────────────────────────────────

    void Move()
    {
        // Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f); // Normalizo

        // Sin control en el aire
        if (!_cc.isGrounded)
            input = Vector3.zero;

        // Dirección relativa a la cámara. Quiero mover mi personaje con respecto a mi camara
        Vector3 camForward = _camOrbit.ForwardOnPlane(); // Llamo al metodo para conseguir X , Z de la camara (0 , 0 , 1f) si miro al norte . Es una sola direccion
        Vector3 camRight = new Vector3(camForward.z, 0, -camForward.x); // Cuenta matematica para girar un vector perpendicularmente 90º y asi conseguir movimiento de derecha a izquierda con respecto a mi camara
        Vector3 moveDirection = camForward * input.z + camRight * input.x; // Lo sumo para ir en diagonal pero siempre con respecto a la camara

        // Velocidad
        float currentSpeed = GetCurrentSpeed();

        // Movimiento final
        Vector3 world = moveDirection * currentSpeed;
        if (_cc.isGrounded)
            _lastGroundWorld = world;
        Vector3 horiz = _cc.isGrounded ? world : _lastGroundWorld;
        Vector3 total = horiz + _velocity; // gravedad en velocity.y
        _cc.Move(total * Time.deltaTime);

        _lastHorizontalSpeed = new Vector3(_cc.velocity.x, 0f, _cc.velocity.z).magnitude;

        // === FACING ===

        if (_cc.isGrounded && _rotateToMoveDir && moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation =  Quaternion.Slerp(transform.rotation, targetRot, _faceLerp * Time.deltaTime);
        }
    }



    void UpdateAnim()
    {
        if (!_anim) return;

        // Normalizamos 0..1 respecto a runSpeed
        float speed01 = Mathf.Clamp01(_lastHorizontalSpeed / _runSpeed);

        // Suavizado para que no “salte” la mezcla
        float target = _cc.isGrounded ? speed01 : 0f;
        float dampTime = _cc.isGrounded ? 0.12f : 0f;   // suave en suelo, instantáneo en aire
        _anim.SetFloat("Speed01", target, dampTime, Time.deltaTime);

        _anim.SetBool("IsCrouching", _isCrouching);
        _anim.SetBool("IsGrounded", _cc.isGrounded);


    }

    void UpdateState()
    {
        if (_isCrouching)
            _state = MovementState.Crouch;
        else if (_isRunning)
            _state = MovementState.Run;
        else
            _state = MovementState.Walk;
    }

    float GetCurrentSpeed() // Metodo para conseguir la velocidad actual del personaje
    {
        switch(_state)
        {
            case MovementState.Crouch:
                return _crouchSpeed;        // No hay break por que el return me devuelve lo esperado y sale del switch
            case MovementState.Run:
                return _runSpeed;
            default:
                return _walkSpeed;
        }
    }
    void Crouch()
    {
        if (!_cc.isGrounded) return;

        if (Input.GetKeyDown(_crouchKey))
        {
            _isCrouching = !_isCrouching; // toggle
            float h = _isCrouching ? _crouchingHeight : _standingHeight;
            _cc.height = h;
            _cc.center = new Vector3(0, h / 2f, 0);
        }
    }
    
    void Run()
    {
        if (!_cc.isGrounded) return;

        _isRunning = Input.GetKey(_runKey);
    }

    void Jump() // No creo que se use en este juego
    {
        if (_cc.isGrounded && Input.GetKeyDown(KeyCode.Space) && !_isCrouching)
        {
            _velocity.y = Mathf.Sqrt(2f * _jumpHeight * -_gravity);
        }
    }

    void ApplyGravity()
    {
        if (_cc.isGrounded && _velocity.y < 0) // Velocity siempre es negativo por gravity.
                _velocity.y = -2f;           // Si estas tocando el suelo te deja pegado a el con -2
        _velocity.y += _gravity * Time.deltaTime;
    }


}
