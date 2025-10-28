using UnityEngine;

public enum AnimState
{
    Idle = 0,
    Patrolling = 1,
    Suspicious = 2,
    Danger = 3,
    Scan = 4,
}


public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int IdleHash = Animator.StringToHash("Rifle Idle");
    private static readonly int PatrolHash = Animator.StringToHash("Rifle Patrolling Walk");
    private static readonly int SuspiciousHash = Animator.StringToHash("Rifle Suspicious");
    private static readonly int DangerHash = Animator.StringToHash("Rifle Danger Run");
    private static readonly int ScanHash = Animator.StringToHash("Rifle Scan");
    private static readonly int SpeedHash = Animator.StringToHash("Speed01");
    private AnimState _current;
    int upperBodyLayerIdx;

    EnemyControllerBase _controller;   // ← referencia al emisor

    void Reset()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        _controller = GetComponentInParent<EnemyControllerBase>();
    }

    void OnEnable()
    {
        if (!_controller) _controller = GetComponentInParent<EnemyControllerBase>();
        if (_controller)
        {
            _controller.OnAnimState += HandleState;
            _controller.OnAnimBool += HandleAnimBool;
            _controller.OnAnimTrigger += HandleTrigger;
            _controller.OnSpeed01Changed += HandleSpeed;
        }
    }

    void OnDisable()
    {
        if (_controller)
        {
            _controller.OnAnimState -= HandleState;
            _controller.OnAnimBool -= HandleAnimBool;
            _controller.OnAnimTrigger -= HandleTrigger;
            _controller.OnSpeed01Changed -= HandleSpeed;
        }
    }

    private void Start()
    {
        upperBodyLayerIdx = animator.GetLayerIndex("UpperBody");
    }
    private void HandleSpeed(float norm)
    {
        // damping + deltaTime para que no patine
        animator.SetFloat(SpeedHash, norm, 0.10f, Time.deltaTime);
    }
    void HandleState(AnimState next)
    {
        if (next == _current) return;
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

        animator.CrossFadeInFixedTime(targetHash, 0.15f, 0);
    }

    void HandleTrigger(int triggerHash)
    {
        animator.SetTrigger(triggerHash);

    }
    void HandleAnimBool(int paramHash, bool value)
    {
        animator.SetBool(paramHash, value);
    }

    // helpers públicos si los seguís usando en otro lado (opcionales)
    public void SetLayerWeight(float weight)
    {
        animator.SetLayerWeight(upperBodyLayerIdx, weight);
    }
    public void PlayScanAnimation(int animHash, int layer = 0, float normalizedTime = 0f)
    {
        animator.Play(animHash, layer, normalizedTime);
    }
    public bool IsUpperFiring()
    {
        if (upperBodyLayerIdx < 0) upperBodyLayerIdx = animator.GetLayerIndex("UpperBody");
        var st = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIdx);
        var nx = animator.GetNextAnimatorStateInfo(upperBodyLayerIdx);
        // Usamos tag "Firing" en el clip de Upper_Fire
        return st.IsTag("Firing") || nx.IsTag("Firing");
    }

}
