using UnityEngine;
using static UnityEngine.UI.Image;

public class MeleeAttack : AttackBase
{
    [Header("Melee")]
    [SerializeField] private LineRenderer beamPrefab; // opcional, para “flash”
    [SerializeField] private float beamLife = 0.1f;  // dura 1–2 frames


    protected override void DoAttack(Transform target, Vector3 seenPos)
    {
        if (IsInRange(seenPos))
        {
            var dmg = target.GetComponent<IDamageable>();
            if (dmg != null)
            {
                if (beamPrefab) StartCoroutine(FlashBeam(transform.position + Vector3.up * 2f, target.position));
                dmg.TakeDamage((float)damage);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxRange);
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