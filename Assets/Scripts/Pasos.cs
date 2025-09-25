using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerMovement playerMovement;
    public AudioClip walkClip;
    public AudioClip runClip;
    public AudioClip crouchClip;

    [Header("Tiempos entre pasos")]
    public float walkStepRate = 0.5f;
    public float runStepRate = 0.4f;
    public float crouchStepRate = 0.8f;

    private AudioSource footstepSource;
    private float footstepTimer;

    void Awake()
    {
        footstepSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (playerMovement == null) return;

        bool isGrounded = playerMovementIsGrounded();
        bool isMoving = playerMovementIsMoving();

        if (!isGrounded || !isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        AudioClip currentClip = walkClip;
        float stepRate = walkStepRate;

        if (playerMovement.IsCrouching)
        {
            currentClip = crouchClip;
            stepRate = crouchStepRate;
        }
        else if (playerMovementIsRunning())
        {
            currentClip = runClip;
            stepRate = runStepRate;
        }

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= stepRate)
        {
            footstepSource.PlayOneShot(currentClip);
            footstepTimer = 0f;
        }
    }

    // Métodos para leer datos del script PlayerMovement
    private bool playerMovementIsGrounded()
    {
        // Accede al método privado IsGrounded usando reflexión
        var method = typeof(PlayerMovement).GetMethod("IsGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)method.Invoke(playerMovement, null);
    }

    private bool playerMovementIsRunning()
    {
        // Accede al campo privado _isRunning
        var field = typeof(PlayerMovement).GetField("_isRunning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)field.GetValue(playerMovement);
    }

    private bool playerMovementIsMoving()
    {
        // Se considera que hay movimiento si la velocidad horizontal es significativa
        var field = typeof(PlayerMovement).GetField("_lastHorizontalSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float speed = (float)field.GetValue(playerMovement);
        return speed > 0.1f;
    }
}