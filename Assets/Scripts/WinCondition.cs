using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WinCondition : MonoBehaviour
{
    private void Awake()
    {
        // Asegura que el BoxCollider es un trigger para detectar entradas
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto que entr en el trigger es el jugador.
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("Player ha entrado en la zona de victoria.");

            // Llama al mtodo WinGame() del GameManager para cambiar de escena.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
            else
            {
                UnityEngine.Debug.LogError("GameManager.Instance no encontrado. Asegurese de que el GameManager este en la escena.");
            }
        }
    }
}