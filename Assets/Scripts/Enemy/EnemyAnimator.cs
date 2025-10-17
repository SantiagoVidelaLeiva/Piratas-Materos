using UnityEngine;

public enum AnimState
{
    Idle = 0,
    Patrolling = 1,
    Suspicious = 2,
    Danger = 3,
    Scan = 4,
}

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Hashes para no allocar strings cada frame
    private static readonly int IdleHash = Animator.StringToHash("Rifle Idle");
    private static readonly int PatrolHash = Animator.StringToHash("Rifle Patrolling Walk");
    private static readonly int SuspiciousHash = Animator.StringToHash("Rifle Suspicious");
    private static readonly int DangerHash = Animator.StringToHash("Rifle Danger Run");
    private static readonly int ScanHash = Animator.StringToHash("Rifle Scan");

    private AnimState _current;
    int upperBodyLayerIdx;
    private void Start()
    {
        upperBodyLayerIdx = animator.GetLayerIndex("UpperBody");
    }
    void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public void SetLayerWeight(float weight)
    {
        if (animator)
            animator.SetLayerWeight(upperBodyLayerIdx, weight);
    }
    public void PlayScanAnimation(int animHash, int layer = 0, float normalizedTime = 0f)
    {
        if (animator)
            animator.Play(animHash, layer, normalizedTime);
    }
    public void SetTrigger(int triggerHash)
    {
        if (animator)
            animator.SetTrigger(triggerHash);
    }
    public void SetState(AnimState next, float fade = 0.15f)
    {
        if (next == _current) return; // evita reiniciar la misma anim

        _current = next;

        int targetHash = next switch
        {
            AnimState.Idle => IdleHash,
            AnimState.Patrolling => PatrolHash,
            AnimState.Suspicious => SuspiciousHash,
            AnimState.Danger => DangerHash,
            AnimState.Scan => ScanHash,
            _ => IdleHash
        };

        // CrossFade robusto (independiente de transiciones/conds del Animator)
        animator.CrossFadeInFixedTime(targetHash, fade, layer: 0);
    }
}