using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class RagdollController : MonoBehaviour
{
    [Header("Opciones")]
    public bool startRagdolled = false;
    public float fadeToKinematicAfter = 8f; // opcional: convertir a estático después de Xs

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
        // recolectar componentes en los hijos (excepto body root si deseás)
        _rigidbodies = GetComponentsInChildren<Rigidbody>(includeInactive: true);
        _colliders = GetComponentsInChildren<Collider>(includeInactive: true);

        // llenar lista de partes (evitar incluir el collider del "capsule" del enemy si lo tenes aparte)
        foreach (var rb in _rigidbodies)
        {
            // opcional: saltar el rigidbody del root si ese no es parte del ragdoll
            _parts.Add((rb, rb.GetComponent<Collider>()));
        }

        // inicial: ragdoll desactivado (kinematic true)
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
    //void HandleDeath()
    //{
    //    Vector3 randomForce = Random.onUnitSphere * 2f; // o el impacto real si lo guardás
    //    Vector3 hitPoint = enemyHealth.transform.position + Vector3.up * 0.5f;

    //    ragdollController.MakeRagdoll(randomForce, hitPoint);
    //}

    void SetRagdollState(bool active)
    {
        _isRagdoll = active;

        // Animator
        if (_anim) _anim.enabled = !active;

        // NavMeshAgent
        if (_agent) _agent.enabled = !active;

        // partes físicas
        foreach (var (rb, col) in _parts)
        {
            // Muchas setups usan isKinematic = true para "apagar" física.
            rb.isKinematic = !active;

            // Si querés que al principio los colliders no interfieran con raycasts/physics,
            // podés desactivarlos mientras kinematic = true. Personalmente dejo enabled true.
            if (col) col.enabled = true;
        }

        // Si activamos ragdoll, "despertamos" las rigidbodies para que reaccionen inmediatamente
        if (active)
        {
            foreach (var (rb, _) in _parts)
            {
                rb.WakeUp();
            }
        }
    }

    // Llamar cuando muere
    public void MakeRagdoll(Vector3 force, Vector3 forcePoint)
    {
        if (_isRagdoll) return;

        // opcional: transferir velocidad del agent al ragdoll
        Vector3 transferredVel = Vector3.zero;
        if (_agent != null) transferredVel = _agent.velocity;

        SetRagdollState(true);

        // aplicar la velocidad y fuerza al rigidbody central (hip)
        // Buscá un rigidbody que sea "hips" o "pelvis"
        Rigidbody pelvis = FindPelvis();
        if (pelvis != null)
        {
            pelvis.linearVelocity = transferredVel;
            pelvis.AddForceAtPosition(force, forcePoint, ForceMode.Impulse);
        }
        else
        {
            // si no hay pelvis, aplica a todos ligeramente
            foreach (var (rb, _) in _parts)
            {
                rb.linearVelocity = transferredVel;
                rb.AddForce(force * 0.1f, ForceMode.Impulse);
            }
        }

        // opcional: programar conversión a estático para perf
        if (fadeToKinematicAfter > 0f)
            Invoke(nameof(FadeToKinematic), fadeToKinematicAfter);
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
