using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WinCondition : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }
}