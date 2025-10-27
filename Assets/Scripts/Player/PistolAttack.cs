using UnityEngine;
using UnityEngine.UI;

public class PistolAttack : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CameraOrbit camOrbit;                 // asigná tu CameraOrbit
    [SerializeField] public Transform muzzle;
    [SerializeField] Animator animator;
    static readonly int IsAiming_Hash = Animator.StringToHash("IsAiming");

    [Header("UI / World Reticle")]
    [SerializeField] RectTransform reticleUI;              // opcional (UI)
    [SerializeField] Transform reticleWorld;               // opcional (world-space marcador)
    [SerializeField] bool reticleUITracksHit = false;      // si true, la UI se mueve al hit (útil debug)

    [Header("Masks")]
    [SerializeField] LayerMask aimMask = ~0;

    [Header("Weapon")]
    [SerializeField] float damage = 100f;
    [SerializeField] float maxRange = 100f;
    [SerializeField] float nearWallDistance = 1.0f;

    [Header("FX")]
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject impactPrefab;
    [SerializeField] float impactLife = 3f;
    [SerializeField] LineRenderer beamPrefab;
    [SerializeField] float beamLife = 0.08f;
    [SerializeField] GameObject shootVfxPrefab;
    [SerializeField] float shootVfxLife = 2f;

    // último punto de mira (lo que ve la cámara en el centro)
    public Vector3 LastAimPoint { get; private set; }
    public bool HasAimHit { get; private set; }

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!camOrbit) camOrbit = Camera.main ? Camera.main.GetComponent<CameraOrbit>() : null;

        // asegurá que no te pegues al Player
        int player = LayerMask.NameToLayer("Player");
        if (player >= 0) aimMask &= ~(1 << player);
    }

    void Update()
    {
        if (!camOrbit || !muzzle) return;

        if (animator) animator.SetBool(IsAiming_Hash, Input.GetMouseButton(1));

        // === Aiming (centro de pantalla) ===
        Ray camRay = camOrbit.GetAimRay();
        HasAimHit = Physics.Raycast(camRay, out RaycastHit camHit, maxRange, aimMask, QueryTriggerInteraction.Ignore);
        LastAimPoint = HasAimHit ? camHit.point : camRay.GetPoint(maxRange);

        // Actualizar retículas
        UpdateReticles(camRay, LastAimPoint);

        if (Input.GetButtonDown("Fire1"))
            Shoot(camRay, LastAimPoint);
        Debug.DrawRay(camRay.origin, camRay.direction * maxRange, Color.cyan, 0f);
        Debug.DrawLine(muzzle.position, LastAimPoint, Color.yellow, 0f);
    }

    void UpdateReticles(Ray camRay, Vector3 aimPoint)
    {
        // UI fija al centro (crosshair clásico): dejá tu imagen centrada en el Canvas y listo.
        // Si querés que la UI "persiga" el punto de impacto (útil debug), activá reticleUITracksHit:
        if (reticleUI && reticleUITracksHit && Camera.main)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(aimPoint);
            reticleUI.position = screen;
        }

        // Marcador world-space (si lo usás)
        if (reticleWorld)
        {
            reticleWorld.position = aimPoint;
            // que mire a la cámara para verse “plano”
            var cam = Camera.main;
            if (cam) reticleWorld.rotation = Quaternion.LookRotation(reticleWorld.position - cam.transform.position);
        }
    }

    void Shoot(Ray camRay, Vector3 aimPoint)
    {
        // FX en muzzle
        if (muzzleFlash) StartCoroutine(FlashOnce(muzzleFlash, 0.05f));
        if (shootVfxPrefab)
        {
            var vfx = Instantiate(shootVfxPrefab, muzzle.position, muzzle.rotation);
            var ps = vfx.GetComponent<ParticleSystem>();
            Destroy(vfx, ps ? ps.main.duration + ps.main.startLifetime.constantMax + 0.1f : shootVfxLife);
        }

        // Dirección desde el muzzle hacia el aimPoint (centro de pantalla)
        Vector3 toAim = aimPoint - muzzle.position;
        if (toAim.sqrMagnitude < 1e-4f) toAim = muzzle.forward;

        // Forzar hemisferio (por si algo quedó atrás de la cámara)
        if (Vector3.Dot(toAim, camRay.direction) < 0f)
            toAim = camRay.direction;

        Vector3 dirFromMuzzle = toAim.normalized;

        // Near-wall
        if (Physics.Raycast(muzzle.position, dirFromMuzzle, out RaycastHit closeHit, nearWallDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            ApplyHit(closeHit, muzzle.position);
            return;
        }

        // Tiro definitivo desde el muzzle
        if (Physics.Raycast(muzzle.position, dirFromMuzzle, out RaycastHit finalHit, maxRange, aimMask, QueryTriggerInteraction.Ignore))
            ApplyHit(finalHit, muzzle.position);
        else if (beamPrefab)
            StartCoroutine(FlashBeam(muzzle.position, muzzle.position + dirFromMuzzle * maxRange));
    }

    void ApplyHit(RaycastHit hit, Vector3 from)
    {
        if (impactPrefab)
            Destroy(Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal)), impactLife);

        if (beamPrefab) StartCoroutine(FlashBeam(from, hit.point));

        if (hit.collider.TryGetComponent(out CharacterHealth health))
            health.TakeDamage(damage);
        else
            hit.collider.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        if (hit.rigidbody)
            hit.rigidbody.AddForceAtPosition((hit.point - from).normalized * damage, hit.point, ForceMode.Impulse);
    }

    System.Collections.IEnumerator FlashOnce(ParticleSystem prefab, float dur)
    {
        var ps = Instantiate(prefab, muzzle.position, muzzle.rotation);
        ps.Play();
        yield return new WaitForSeconds(dur);
        Destroy(ps.gameObject);
    }

    System.Collections.IEnumerator FlashBeam(Vector3 a, Vector3 b)
    {
        var beam = Instantiate(beamPrefab);
        beam.positionCount = 2;
        beam.SetPosition(0, a);
        beam.SetPosition(1, b);
        yield return null;
        yield return new WaitForSeconds(beamLife);
        Destroy(beam.gameObject);
    }
}
