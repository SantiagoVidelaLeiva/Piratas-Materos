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
    [SerializeField] private MonoBehaviour playerMovement;

    [Header("Player Aim / IK / Cámara que deforman el cuerpo")]
    [SerializeField] private MonoBehaviour aimController;
    [SerializeField] private MonoBehaviour cameraOrbit;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private MonoBehaviour aimDriver;
    [SerializeField] private Transform cameraRig;
    [Header("Enemy References")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private MonoBehaviour enemyController;
    [SerializeField] private CharacterHealth enemyHealth;

    [Header("Animation Triggers")]
    [SerializeField] private string playerTrigger = "Takedown";
    [SerializeField] private string enemyTrigger = "TakedownVictim";

    [Header("Takedown Positioning")]
    private float behindDistance = 2.6f;
    [SerializeField] private float heightAlign = 0f;
    [SerializeField] private float visualYOffset = -1.059f;
        private Vector3 camSavedPos;
    private Quaternion camSavedRot;
    [Header("Timing")]
    private float takedownDuration = 17.567f;

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
        if (cameraRig != null && playerRoot != null)
        {
            camSavedPos = playerRoot.InverseTransformPoint(cameraRig.position);
            camSavedRot = Quaternion.Inverse(playerRoot.rotation) * cameraRig.rotation;
        }

        Vector3 enemyPos = enemyRoot.position;
        Quaternion enemyRot = enemyRoot.rotation;

        Vector3 playerTargetPos = enemyPos - enemyRoot.forward * behindDistance;
        playerTargetPos.y = enemyPos.y + heightAlign + visualYOffset;

        Quaternion playerTargetRot = enemyRoot.rotation * Quaternion.AngleAxis(22f, Vector3.up);

        AlignDynamic(playerRoot, playerTargetPos, playerTargetRot);
        AlignDynamic(enemyRoot, enemyPos, enemyRot);


        if (playerMovement) playerMovement.enabled = false;
        if (enemyController) enemyController.enabled = false;


        var agent = enemyRoot.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;


        playerAnimator.enabled = true;
        playerAnimator.applyRootMotion = true;
        enemyAnimator.applyRootMotion = true;

        playerAnimator.SetTrigger(playerTrigger);
        enemyAnimator.SetTrigger(enemyTrigger);

        SetExtraPoseSystemsEnabled(false);

        StartCoroutine(TakedownFinishTimer());
    }

    private System.Collections.IEnumerator TakedownFinishTimer()
    {
        float timer = takedownDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        FinishTakedown();
    }

    private void FinishTakedown()
    {
        if (enemyHealth != null)
        {
            enemyHealth.DieWithoutRagdoll();
        }
        if (cameraRig != null && playerRoot != null)
        {
            Vector3 newWorldPos = playerRoot.TransformPoint(camSavedPos);
            Quaternion newWorldRot = playerRoot.rotation * camSavedRot;

            cameraRig.SetPositionAndRotation(newWorldPos, newWorldRot);
        }
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
            rootRb.isKinematic = true;
        }

        playerAnimator.applyRootMotion = false;
        lockRootRotation = false;

        if (playerMovement) playerMovement.enabled = true;
        SetExtraPoseSystemsEnabled(true);

        isPerformingTakedown = false;
    }

    private void OnAnimatorMove()
    {
        if (!isPerformingTakedown) return;
        if (playerAnimator == null || !playerAnimator.applyRootMotion) return;

        Vector3 deltaPos = playerAnimator.deltaPosition;
        playerRoot.position += deltaPos;

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
        if (aimDriver) aimDriver.enabled = enabled;
        if (rigBuilder) rigBuilder.enabled = enabled;

        if (aimController) aimController.enabled = enabled;
        if (cameraOrbit) cameraOrbit.enabled = enabled;
    }
}
