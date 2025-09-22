using UnityEngine;

public class RangedAttack : AttackBase
{
    [Header("Mid-Ranged")]
    [SerializeField] private float spreadDegrees = 2.5f;
    [SerializeField] private LineRenderer beamPrefab; // opcional, para “flash”
    [SerializeField] private float beamLife = 0.1f;  // dura 1–2 frames
    private void Awake()
    {
        beamPrefab = GameObject.Find("RedLineRender").GetComponent<LineRenderer>();
        firePoint = transform.Find("Eyes");
    }
    protected override void DoAttack(Transform target, Vector3 seenPos)
    {
        Vector3 origin = firePoint ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 adjustTarget = seenPos + Vector3.down * 0.2f; // Dispara al cuerpo
        Vector3 dir = (adjustTarget - origin).normalized;
        

        // spread
        dir = Quaternion.Euler(Random.Range(-spreadDegrees, spreadDegrees),
                               Random.Range(-spreadDegrees, spreadDegrees),
                               0f) * dir;

        if (Physics.Raycast(origin, dir, out var hit, maxRange, hitMask))
        {
            // daño
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
            if (beamPrefab) StartCoroutine(FlashBeam(origin, hit.point));
        }
        else
        {
            if (beamPrefab) StartCoroutine(FlashBeam(origin, origin + dir * maxRange));
        }

        Debug.DrawRay(origin, dir * maxRange, Color.red, 0.05f);
    }

    private System.Collections.IEnumerator FlashBeam(Vector3 a, Vector3 b)
    {
        var beam = Instantiate(beamPrefab, a, Quaternion.identity);
        beam.positionCount = 2;
        beam.SetPosition(0, a);
        beam.SetPosition(1, b);
        yield return null;                           // 1 frame
        yield return new WaitForSeconds(beamLife);   // breve
        Destroy(beam.gameObject);
    }
}