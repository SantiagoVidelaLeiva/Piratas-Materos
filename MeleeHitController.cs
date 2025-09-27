using UnityEngine;

public class MeleeHitController : MonoBehaviour
{
    private Collider hitColl;

    void Start()
    {
        hitColl = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemyHealth = other.GetComponent<CharacterHealth>();
            enemyHealth.TakeDamage(25f);
        }
    }

    public void EnableHitColl()
    {
        hitColl.enabled = true;
    }

    public void DisableHitColl()
    {
        hitColl.enabled = false;
    }
}
