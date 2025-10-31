using UnityEngine;

public class PistolPickup : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0f, 50f * Time.deltaTime, 0f);
    }
    private void OnTriggerEnter(Collider other)
    {
        var pistolAttack = other.GetComponentInChildren<PistolAttack>();
        if (pistolAttack != null)
        {
            pistolAttack.GivePistol();          // activa la pistola
            Destroy(gameObject);                // elimina el objeto flotante
        }
    }
}