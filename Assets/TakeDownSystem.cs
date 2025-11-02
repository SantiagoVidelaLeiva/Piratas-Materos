using UnityEngine;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(BoxCollider))]
public class EnemyTakedown : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] private string _interactPrompt = "E to takedown";

    [Header("Player References")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private MonoBehaviour playerMovement; // tu script de moverte caminando/corriendo

    [Header("Player Aim / IK / Cámara que deforman el cuerpo")]
    [SerializeField] private MonoBehaviour aimController;   // script que rota el cuerpo al apuntar
    [SerializeField] private MonoBehaviour cameraOrbit;     // mira mouse / hombro
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private MonoBehaviour aimDriver; // tu AimIKDriver

    [Header("Enemy References")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private MonoBehaviour enemyController; // IA/NavMesh/etc.
    [SerializeField] private CharacterHealth enemyHealth;

    [Header("Animation Triggers")]
    [SerializeField] private string playerTrigger = "Takedown";
    [SerializeField] private string enemyTrigger = "TakedownVictim";

    [Header("Takedown Positioning")]
    private float behindDistance = 2.6f;
    [SerializeField] private float heightAlign = 0f;
    [SerializeField] private float visualYOffset = -1.059f;

    [Header("Timing")]
    private float takedownDuration = 17.567f; // <<< NUEVO >>> cuántos segundos dura la ejecución antes de terminar

    // runtime state
    private bool _hasBeenUsed = false;
    private bool isPerformingTakedown = false;
    private bool lockRootRotation = false;

    public string InteractPrompt => _interactPrompt;

    private void Awake()
    {
        if (enemyRoot == null)
            enemyRoot = GetComponentInParent<Transform>();

        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<CharacterHealth>();

        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    // llamado por Interactable cuando apretás E
    public bool Interact()
    {
        if (_hasBeenUsed) return false;
        _hasBeenUsed = true;

        StartTakedownCinematic();

        return true;
    }

    private void StartTakedownCinematic()
    {
        isPerformingTakedown = true;
        lockRootRotation = true;

        // 1. calcular posiciones sync
        Vector3 enemyPos = enemyRoot.position;
        Quaternion enemyRot = enemyRoot.rotation;

        Vector3 playerTargetPos = enemyPos - enemyRoot.forward * behindDistance;
        playerTargetPos.y = enemyPos.y + heightAlign + visualYOffset;

        // mirada casi paralela al enemigo, con un leve offset de yaw
        Quaternion playerTargetRot = enemyRoot.rotation * Quaternion.AngleAxis(22f, Vector3.up);

        AlignDynamic(playerRoot, playerTargetPos, playerTargetRot);
        AlignDynamic(enemyRoot, enemyPos, enemyRot);

        // 2. desactivar control del jugador e IA del enemigo
        if (playerMovement) playerMovement.enabled = false;
        if (enemyController) enemyController.enabled = false;

        // si tiene NavMeshAgent lo podrías apagar acá también si querés
        var agent = enemyRoot.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        // 3. root motion ON
        playerAnimator.enabled = true;
        playerAnimator.applyRootMotion = true;
        enemyAnimator.applyRootMotion = true;

        // 4. disparar animaciones sincronizadas
        playerAnimator.SetTrigger(playerTrigger);
        enemyAnimator.SetTrigger(enemyTrigger);

        // 5. apagar IK / rig de apuntado para que no te tuerza el cuerpo
        SetExtraPoseSystemsEnabled(false);

        // 6. programar el final dentro de X segundos
        StartCoroutine(TakedownFinishTimer()); // <<< NUEVO >>>
    }

    // <<< NUEVO >>> corrutina que espera y termina el takedown
    private System.Collections.IEnumerator TakedownFinishTimer()
    {
        float timer = takedownDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // cuando termina el tiempo, cerramos la ejecución
        FinishTakedown();
    }

    // mata al enemigo, restaura control jugador, limpia estados
    private void FinishTakedown()
    {
        // 1. marcar muerte gameplay
        if (enemyHealth != null)
        {
            enemyHealth.DieWithoutRagdoll(); // esto baja la vida, marca isDead, etc.
        }

        // 2. apagar IA y navegación
        if (enemyController) enemyController.enabled = false;

        var agent = enemyRoot.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        var mainCapsule = enemyRoot.GetComponent<CapsuleCollider>();
        if (mainCapsule) mainCapsule.enabled = false;

        var rootRb = enemyRoot.GetComponent<Rigidbody>();
        if (rootRb)
        {
            rootRb.linearVelocity = Vector3.zero;
            rootRb.angularVelocity = Vector3.zero;
            rootRb.isKinematic = true; // así ya no te empuja
        }

        // 5. devolver control al jugador
        playerAnimator.applyRootMotion = false;
        lockRootRotation = false;

        if (playerMovement) playerMovement.enabled = true;
        SetExtraPoseSystemsEnabled(true);

        isPerformingTakedown = false;
    }

    // seguir aplicando root motion del player mientras está en la cinemática
    private void OnAnimatorMove()
    {
        if (!isPerformingTakedown) return;
        if (playerAnimator == null || !playerAnimator.applyRootMotion) return;

        // mover al player según la anim
        Vector3 deltaPos = playerAnimator.deltaPosition;
        playerRoot.position += deltaPos;

        // controlar rotación para que no se desvíe loco
        if (lockRootRotation && enemyRoot != null)
        {
            playerRoot.rotation = Quaternion.LookRotation(enemyRoot.forward, Vector3.up);
        }
        else
        {
            playerRoot.rotation *= playerAnimator.deltaRotation;
        }
    }

    private void AlignDynamic(Transform t, Vector3 worldPos, Quaternion worldRot)
    {
        Rigidbody rb = t.GetComponent<Rigidbody>();

        // si el jugador tiene rigidbody dinámico, movemos con MovePosition/MoveRotation
        if (rb != null && rb.isKinematic == false)
        {
            rb.MovePosition(worldPos);
            rb.MoveRotation(worldRot);
        }
        else
        {
            t.SetPositionAndRotation(worldPos, worldRot);
        }
    }

    private void SetExtraPoseSystemsEnabled(bool enabled)
    {
        // apaga y prende sistemas que deforman la pose en runtime
        if (aimDriver) aimDriver.enabled = enabled;
        if (rigBuilder) rigBuilder.enabled = enabled;

        // si querés también apagar la lógica de apuntar cuerpo / cámara mientras dura el takedown:
        if (aimController) aimController.enabled = enabled;
        if (cameraOrbit) cameraOrbit.enabled = enabled;
    }
}
