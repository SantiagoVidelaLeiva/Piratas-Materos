using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    [SerializeField] private GameObject distractionPrefab;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private float throwForce = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) // tirar con la G por ejemplo
        {
            GameObject obj = Instantiate(distractionPrefab, throwOrigin.position, throwOrigin.rotation);
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(throwOrigin.forward * throwForce, ForceMode.Impulse);
            }
        }
    }
}