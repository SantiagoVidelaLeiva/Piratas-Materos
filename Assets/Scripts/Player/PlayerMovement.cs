using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour, IHeightProvider
{
    [Header("Facing")]
    [SerializeField] CameraOrbit _camOrbit;
    [SerializeField] float _faceLerp = 8f;
    [SerializeField] Transform _target;

    [Header("Animación")]
    [SerializeField] Animator _anim;
    [SerializeField] bool _rotateToMoveDir = true;

    [Header("Visual (offset)")]
    [SerializeField] Transform _visual;
    [SerializeField] float _visualYOffset = -0.04f;

    [Header("Movimiento")]
    Rigidbody _rb;
    CapsuleCollider _col;
    [SerializeField] float _walkSpeed = 4.5f;
    [SerializeField] float _crouchSpeed = 1.2f;
    [SerializeField] float _runSpeed = 6.5f;


    [Header("Inputs")]
    KeyCode _runKey = KeyCode.LeftShift;
    KeyCode _attackInput = KeyCode.X;

    [Header("Agacharse")]
    KeyCode _crouchKey = KeyCode.LeftControl;
    [SerializeField] float _standingHeight = 1.8f;
    [SerializeField] float _crouchingHeight = 1.2f;
    public bool IsCrouching { get; private set; }

    [Header("Scripts")]
    [SerializeField] PlayerGravity _playerGravity;

    public float GetEyeHeight() => IsCrouching ? _crouchingHeight : _standingHeight;

    enum MovementState { Walk, Run, Crouch }
    MovementState _state;

    bool _isRunning;
    float _lastHorizontalSpeed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;   // para no volcar
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        _col = GetComponent<CapsuleCollider>();
        _col.height = _standingHeight;
        _col.center = new Vector3(0, _standingHeight / 2f, 0);

        if (!_anim) _anim = GetComponentInChildren<Animator>();
        if (_anim) _anim.applyRootMotion = false;

        if (!_visual && _anim) _visual = _anim.transform;
        if (_visual) _visual.localPosition = new Vector3(0f, _visualYOffset, 0f);

        if (!_playerGravity) _playerGravity = GetComponent<PlayerGravity>();
    }

    void Update()
    {
        UpdateState();
        Crouch();
        Run();
        Attack();
        UpdateAnim();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f); // Normalizo.

        // Dirección relativa a la cámara. Quiero mover mi personaje con respecto a mi camara
        Vector3 camForward = _camOrbit.ForwardOnPlane(); // Llamo al metodo para conseguir X , Z de la camara (0 , 0 , 1f) si miro al norte . Es una sola direccion
        Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized; // Cross da el producto cruzado, es una cuenta matematica para girar un vector perpendicularmente 90º
                                                                             // y asi conseguir movimiento de derecha a izquierda con respecto a mi camara
        Vector3 moveDirection = camForward * input.z + camRight * input.x; // Lo sumo para ir en diagonal pero siempre con respecto a la camara

        float currentSpeed = GetCurrentSpeed();
        Vector3 horiz = moveDirection * currentSpeed;

        Vector3 total = horiz;

        _rb.MovePosition(_rb.position + total * Time.fixedDeltaTime);
        _lastHorizontalSpeed = horiz.magnitude;

        if (_rotateToMoveDir && moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _faceLerp * Time.fixedDeltaTime);
        }
    }

    void UpdateAnim()
    {
        if (!_anim) return;

        float speed01 = Mathf.Clamp01(_lastHorizontalSpeed / _runSpeed);
        float target = IsGrounded() ? speed01 : 0f;
        float dampTime = IsGrounded() ? 0.12f : 0f;

        _anim.SetFloat("Speed01", target, dampTime, Time.deltaTime);
        _anim.SetBool("IsCrouching", IsCrouching);
        _anim.SetBool("IsGrounded", IsGrounded());
    }

    void UpdateState()
    {
        if (IsCrouching)
            _state = MovementState.Crouch;
        else if (_isRunning)
            _state = MovementState.Run;
        else
            _state = MovementState.Walk;
    }

    float GetCurrentSpeed()
    {
        switch (_state)
        {
            case MovementState.Crouch:
                return _crouchSpeed;
            case MovementState.Run:
                return _runSpeed;
            default:
                return _walkSpeed;
        }
    }

    void Crouch()
    {
        if (!IsGrounded()) return;

        if (Input.GetKeyDown(_crouchKey))
        {
            IsCrouching = !IsCrouching;
            float h = IsCrouching ? _crouchingHeight : _standingHeight;
            _col.height = h;
            _col.center = new Vector3(0, h / 2f, 0);
        }
    }

    void Run()
    {
        if (!IsGrounded()) return;
        _isRunning = Input.GetKey(_runKey);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
    }
    
    void Attack()
    {
        if (Input.GetKeyDown(_attackInput))
        {
            _anim.SetTrigger("Attack");
        }
    }
}
