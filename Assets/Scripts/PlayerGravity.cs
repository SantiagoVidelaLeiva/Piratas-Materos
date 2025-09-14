using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerGravity : MonoBehaviour
{
    [Header("Gravedad")]
    [SerializeField] float _gravity = -20f;
    [SerializeField] float _jumpHeight = 1.4f;

    private CharacterController _cc;

    // Esta es ahora una variable privada que podemos modificar directamente
    private Vector3 _verticalVelocity;

    // Esta es la propiedad pública que otros scripts pueden leer
    public Vector3 VerticalVelocity
    {
        get { return _verticalVelocity; }
        private set { _verticalVelocity = value; }
    }

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        ApplyGravity();

        // Aplica el movimiento vertical del CharacterController
        _cc.Move(VerticalVelocity * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        // Si está en el suelo y la velocidad vertical es negativa, se "pega" al suelo.
        if (_cc.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity = new Vector3(0, -2f, 0);
        }

        // Aplica la fuerza de gravedad continuamente
        _verticalVelocity.y += _gravity * Time.deltaTime;
    }

    public void Jump()
    {
        if (_cc.isGrounded)
        {
            _verticalVelocity.y = Mathf.Sqrt(2f * _jumpHeight * -_gravity);
        }
    }
}