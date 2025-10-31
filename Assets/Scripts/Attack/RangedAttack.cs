using UnityEngine;

public class RangedAttack : AttackBase
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float shootSoundCooldown = 0.58f;
    private float _nextShootSoundTime = 0f;


    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        // intentar autoconseguir el FirePoint si está en hijos inactivos
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
        if (_data == null)
        {
            Debug.LogWarning($"{name} no tiene AttackData asignado");
            return;
        }

        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.5f;

        Vector3 adjustTarget = seenPos + Vector3.down * 0.2f;
        Vector3 dir = (adjustTarget - origin).normalized;

        float spread = _data.spreadDegrees;
        dir = Quaternion.Euler(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        ) * dir;

        if (Time.time >= _nextShootSoundTime && audioSource)
        {
            AudioClip clip = _data.attackSFX;
            if (clip)
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
                _nextShootSoundTime = Time.time + shootSoundCooldown;
            }
        }

        if (_data.muzzleFlashPrefab && firePoint)
        {
            var flash = Instantiate(_data.muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
            Destroy(flash, _data.muzzleFlashLife);
        }

        float range = _data.maxRange;
        if (Physics.Raycast(origin, dir, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(_data.baseDamage);
        }
    }

    public override bool CanAttack(Transform target, Vector3 seenPos)
    {
        if (_data == null) return false;

        // respetar cooldown
        if (Time.time < _nextAttackTime) return false;

        // línea de visión directa al player
        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 dir = (seenPos - origin).normalized;

        float distToTarget = Vector3.Distance(origin, seenPos);
        float maxDist = Mathf.Min(_data.maxRange, distToTarget + 0.2f);

        if (Physics.Raycast(origin, dir, out var hit, maxDist, hitMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.transform.root == target.root;
        }

        return false;
    }
}
