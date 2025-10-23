using UnityEngine;
using static UnityEngine.UI.Image;

public class PistolAttack : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Punto desde donde sale el rayo (un empty GameObject en la boca del arma).")]
    public Transform muzzle;

    [Header("Weapon")]
    public float damage = 100f;
    public float range = 100f;

    [Header("Optional FX")]
    public ParticleSystem muzzleFlash;
    public GameObject impactPrefab;
    public float impactLife = 3f;
    [SerializeField] private LineRenderer beamPrefab;
    private float beamLife = 0.1f;
    private void Awake()
    {
        beamPrefab = GameObject.Find("RedLineRender").GetComponent<LineRenderer>();
    }
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            Shoot();
    }

    void Shoot()
    {
        if (muzzle == null)
        {
            Debug.LogWarning("SimpleRayPistol: asigná el muzzle (empty) en el inspector.");
            return;
        }

        // play muzzle
        if (muzzleFlash != null) muzzleFlash.Play();

        Ray ray = new Ray(muzzle.position, muzzle.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // FX de impacto opcional
            if (impactPrefab != null)
            {
                var go = Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(go, impactLife);
            }
            if (beamPrefab) StartCoroutine(FlashBeam(muzzle.position, hit.point));

            // 1) Si el objeto tiene EnemyHealth (tu clase), llamamos a la firma con punto y fuerza si existe
            var enemyHealth = hit.collider.GetComponentInParent<CharacterHealth>();
            if (enemyHealth != null)
            {
                // Intentamos llamar al overload con punto y fuerza si está implementado
                try
                {
                    // Asumimos que la firma es: TakeDamage(float amount, Vector3 hitPoint, Vector3 hitForce)
                    enemyHealth.TakeDamage1(damage, hit.point, muzzle.forward * damage);
                }
                catch (System.MissingMethodException)
                {
                    // si no existe esa firma, probamos la clásica de un solo parámetro (si existe)
                    enemyHealth.TakeDamage(damage);
                }
                return;
            }

            // 2) Fallback simple: TrySendMessage para TakeDamage(float)
            // Esto llamará a cualquier método TakeDamage(float) en el objeto o sus padres (no lanza error si no existe)
            hit.collider.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            // 3) Aplicar impulso si hay rigidbody
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(muzzle.forward * damage, hit.point, ForceMode.Impulse);
            }
        }
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