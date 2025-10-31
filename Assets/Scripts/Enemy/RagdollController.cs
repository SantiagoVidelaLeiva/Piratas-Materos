using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class RagdollController : MonoBehaviour
{
    [Header("Opciones")]
    public bool startRagdolled = false;
    public float fadeToKinematicAfter = 8f;

    Animator _anim;
    NavMeshAgent _agent;
    Collider[] _colliders;
    Rigidbody[] _rigidbodies;
    List<(Rigidbody rb, Collider col)> _parts = new List<(Rigidbody, Collider)>();
    private CharacterHealth _enemyHealth;
    bool _isRagdoll = false;


    void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        _agent = GetComponentInParent<NavMeshAgent>();
        _enemyHealth = GetComponentInParent<CharacterHealth>();
        _rigidbodies = GetComponentsInChildren<Rigidbody>(includeInactive: true);
        _colliders = GetComponentsInChildren<Collider>(includeInactive: true);

        foreach (var rb in _rigidbodies)
        {
            _parts.Add((rb, rb.GetComponent<Collider>()));
        }

        SetRagdollState(startRagdolled);
    }
    void OnEnable()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnDied += HandleDeath;
    }

    void OnDisable()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnDied -= HandleDeath;
    }
    private void HandleDeath()
    {
        Debug.Log("💀 RagdollController recibió evento OnDied");
        Vector3 force = _enemyHealth.LastHitForce;
        Vector3 hitPoint = _enemyHealth.LastHitPoint;
        var capsule = GetComponentInParent<CapsuleCollider>();
        if (capsule) capsule.enabled = false;
        MakeRagdoll(force, hitPoint);
    }


    void SetRagdollState(bool active)
    {
        _isRagdoll = active;

        if (_anim)
        {
            _anim.enabled = !active;
            _anim.updateMode = AnimatorUpdateMode.Normal;
        }


        if (_agent) _agent.enabled = !active;

        foreach (var (rb, col) in _parts)
        {
            rb.isKinematic = !active;

            if (col) col.enabled = true;
        }

        if (active)
        {
            foreach (var (rb, _) in _parts)
            {
                rb.WakeUp();
            }
        }
    }

    public void MakeRagdoll(Vector3 force, Vector3 forcePoint)
    {
        if (_isRagdoll) return;

        Vector3 transferredVel = _agent ? _agent.velocity : Vector3.zero;
        SetRagdollState(true);

        var targetRB = _enemyHealth.LastHitRB;
        if (targetRB == null) targetRB = FindPelvis();

        if (targetRB != null)
        {
            targetRB.linearVelocity = transferredVel;
            targetRB.AddForceAtPosition(force, forcePoint, ForceMode.Impulse);
        }
    }


    Rigidbody FindPelvis()
    {
        foreach (var (rb, _) in _parts)
        {
            if (rb.name.ToLower().Contains("hip") || rb.name.ToLower().Contains("pelvis") || rb.name.ToLower().Contains("root"))
                return rb;
        }
        // fallback
        if (_parts.Count > 0) return _parts[0].rb;
        return null;
    }

    void FadeToKinematic()
    {
        // opcional: para ahorrar CPU podes dejar solo un rigidbody activo o poner todos kinematic
        foreach (var (rb, _) in _parts)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }
}
