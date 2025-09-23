using UnityEngine;

public class SniperAttack : AttackBase
{
    [Header("High-Ranged")]
    [SerializeField] private float aimTime = 0.2f;
    [SerializeField] private float spreadDegrees = 2.5f;
    [SerializeField] private LineRenderer beamPrefab;
    [SerializeField] private float beamLife = 0.1f;

    private void Awake()
    {
        beamPrefab = GameObject.Find("YellowLineRender").GetComponent<LineRenderer>();
        firePoint = transform.Find("Eyes");
    }

    protected override void DoAttack(Transform target, Vector3 seenPos)
    {

        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 adjustTarget = seenPos + Vector3.down * 0.2f;
        Vector3 dir = (adjustTarget - origin).normalized;

        dir = Quaternion.Euler(Random.Range(-spreadDegrees, spreadDegrees),
                               Random.Range(-spreadDegrees, spreadDegrees),
                               0f) * dir;

        if (Physics.Raycast(origin, dir, out var hit, maxRange, hitMask))
        {
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
            if (beamPrefab) StartCoroutine(FlashBeam(origin, hit.point));
        }
        else
        {
            if (beamPrefab) StartCoroutine(FlashBeam(origin, origin + dir * maxRange));
        }

        Debug.DrawRay(origin, dir * maxRange, Color.yellow, 0.05f);
    }

    private System.Collections.IEnumerator FlashBeam(Vector3 a, Vector3 b)
    {
        var beam = Instantiate(beamPrefab, a, Quaternion.identity);
        beam.positionCount = 2;
        beam.SetPosition(0, a);
        beam.SetPosition(1, b);
        yield return null;
        yield return new WaitForSeconds(beamLife);
        Destroy(beam.gameObject);
    }
}