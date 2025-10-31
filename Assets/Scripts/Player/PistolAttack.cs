using UnityEngine;
using UnityEngine.UI;

public class PistolAttack : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CameraOrbit camOrbit;
    [SerializeField] public Transform muzzle;
    [SerializeField] Animator animator;
    static readonly int IsAiming_Hash = Animator.StringToHash("IsAiming");

    [Header("UI / World Reticle")]
    [SerializeField] RectTransform reticleUI;
    [SerializeField] Transform reticleWorld;
    [SerializeField] bool reticleUITracksHit = false;

    [Header("Masks")]
    [SerializeField] LayerMask aimMask = ~0;

    [Header("Weapon")]
    float damage = 50f;
    [SerializeField] float maxRange = 100f;
    [SerializeField] float nearWallDistance = 1.0f;
    [Header("Fire")]
    [SerializeField] float fireRate = 2f;
    float _nextFireTime = 0f;

    [Header("FX")]
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] GameObject impactPrefab;
    [SerializeField] float impactLife = 3f;
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shootClip;
    [Header("Pickup")]
    [SerializeField] bool hasPistol = false;
    public Vector3 LastAimPoint { get; private set; }
    public bool HasAimHit { get; private set; }

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!camOrbit) camOrbit = Camera.main ? Camera.main.GetComponent<CameraOrbit>() : null;
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        if (!muzzle)
        {
            muzzle = System.Array.Find(
                transform.GetComponentsInChildren<Transform>(true),
                t => t.name == "FirePoint"
            );
        }
    }
    void Update()
    {
        if (!camOrbit || !muzzle) return;
        if (!hasPistol)
        {
            if (animator) animator.SetBool(IsAiming_Hash, false);
            return;
        }
        if (animator) animator.SetBool(IsAiming_Hash, Input.GetMouseButton(1));

        Ray camRay = camOrbit.GetAimRay();
        HasAimHit = Physics.Raycast(camRay, out RaycastHit camHit, maxRange, aimMask, QueryTriggerInteraction.Ignore);
        LastAimPoint = HasAimHit ? camHit.point : camRay.GetPoint(maxRange);

        UpdateReticles(camRay, LastAimPoint);

        if (Input.GetButtonDown("Fire1"))
            Shoot(camRay, LastAimPoint);
        Debug.DrawRay(camRay.origin, camRay.direction * maxRange, Color.cyan, 0f);
        Debug.DrawLine(muzzle.position, LastAimPoint, Color.yellow, 0f);
        if (reticleUI)
        {
            bool aiming = Input.GetMouseButton(1);
            reticleUI.gameObject.SetActive(aiming);
        }
    }
    public void GivePistol()
    {
        hasPistol = true;
    }
    void UpdateReticles(Ray camRay, Vector3 aimPoint)
    {
        if (reticleUI && reticleUITracksHit && Camera.main)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(aimPoint);
            reticleUI.position = screen;
        }

        if (reticleWorld)
        {
            reticleWorld.position = aimPoint;
            var cam = Camera.main;
            if (cam) reticleWorld.rotation = Quaternion.LookRotation(reticleWorld.position - cam.transform.position);
        }
    }

    void Shoot(Ray camRay, Vector3 aimPoint)
    {
        if (Time.time < _nextFireTime) return;
        _nextFireTime = Time.time + 1f / fireRate;
        if (muzzleFlash) StartCoroutine(FlashOnce(muzzleFlash, 0.05f));
        if (audioSource && shootClip)
            audioSource.PlayOneShot(shootClip);

        Vector3 toAim = aimPoint - muzzle.position;
        if (toAim.sqrMagnitude < 1e-4f) toAim = muzzle.forward;

        if (Vector3.Dot(toAim, camRay.direction) < 0f)
            toAim = camRay.direction;

        Vector3 dirFromMuzzle = toAim.normalized;

        if (Physics.Raycast(muzzle.position, dirFromMuzzle, out RaycastHit closeHit, nearWallDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            ApplyHit(closeHit, muzzle.position);
            return;
        }

        if (Physics.Raycast(muzzle.position, dirFromMuzzle, out RaycastHit finalHit, maxRange, aimMask, QueryTriggerInteraction.Ignore))
            ApplyHit(finalHit, muzzle.position);

    }

    void ApplyHit(RaycastHit hit, Vector3 from)
    {
        if (impactPrefab)
        {
            bool hitEnemy = hit.collider.GetComponentInParent<CharacterHealth>() != null;

            if (!hitEnemy)
            {
                var rot = Quaternion.LookRotation(hit.normal);
                var fx = Instantiate(impactPrefab, hit.point, rot);
                Destroy(fx, impactLife);
            }
        }


        Vector3 dir = (hit.point - from).normalized;
        float impulsePower = damage * 0.1f;  // ajustá a gusto
        Vector3 force = dir * impulsePower;

        Rigidbody hitRB = hit.rigidbody ? hit.rigidbody : hit.collider.attachedRigidbody;
        if (!hitRB) hitRB = hit.collider.GetComponentInParent<Rigidbody>();
        float appliedDamage = hit.collider.CompareTag("Head") ? 100f : damage;
        Debug.Log($"Hit: {hit.collider.name}, Tag: {hit.collider.tag}, Point: {hit.point}");
        Debug.DrawLine(from, hit.point, Color.red, 2f);
        var health = hit.collider.GetComponentInParent<CharacterHealth>();
        if (health != null)
            health.TakeDamage1(appliedDamage, hit.point, force, hitRB);
        else
            hit.collider.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }


    System.Collections.IEnumerator FlashOnce(ParticleSystem prefab, float dur)
    {
        var ps = Instantiate(prefab, muzzle.position, muzzle.rotation);
        ps.Play();
        yield return new WaitForSeconds(dur);
        Destroy(ps.gameObject);
    }


}
