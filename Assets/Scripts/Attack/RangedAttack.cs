using UnityEngine;

public class RangedAttack : AttackBase
{
    [Header("Mid-Ranged")]
    [SerializeField] private float spreadDegrees = 2.5f;
    [Header("FX")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float muzzleFlashLife = 0.1f;
    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shootClip;
    float _nextShootSoundTime = 0f;
    float shootSoundCooldown = 0.58f;
    protected override void Awake()
    {
        base.Awake();

    }
    void Start()
    {

        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!firePoint)
        {
            firePoint = System.Array.Find(
                transform.GetComponentsInChildren<Transform>(true),
                t => t.name == "FirePoint"
            );
        }
    }
    protected override void DoAttack(Transform target, Vector3 seenPos)
    {
        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 adjustTarget = seenPos + Vector3.down * 0.2f; // Dispara al cuerpo
        Vector3 dir = (adjustTarget - origin).normalized;

        if (Time.time >= _nextShootSoundTime && audioSource && shootClip)
        {
            audioSource.Stop();
            audioSource.clip = shootClip;
            audioSource.Play();
            _nextShootSoundTime = Time.time + shootSoundCooldown;
        }
        // spread
        dir = Quaternion.Euler(Random.Range(-spreadDegrees, spreadDegrees),
                               Random.Range(-spreadDegrees, spreadDegrees),
                               0f) * dir;
        if (muzzleFlashPrefab && firePoint)
        {
            var flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
            Destroy(flash, muzzleFlashLife);
        }
        if (Physics.Raycast(origin, dir, out var hit, maxRange, hitMask))
        {
            // daño
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
        }



    }
    public override bool CanAttack(Transform target, Vector3 seenPos)
    {
        if (Time.time < _nextAttackTime) return false;
        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 dir = (seenPos - origin).normalized;
        float maxDist = Mathf.Min(maxRange, Vector3.Distance(origin, seenPos) + 0.2f);
        if (Physics.Raycast(origin, dir, out var hit, maxDist, hitMask, QueryTriggerInteraction.Ignore))
            return hit.collider.transform.root == target.root;

        return false;
    }

}