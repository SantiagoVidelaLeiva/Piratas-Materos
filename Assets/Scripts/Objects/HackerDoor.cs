using UnityEngine;

public class HackerDoor : MonoBehaviour
{
    private bool _isHacked = false;

    public void OpenDoor()
    {
        if (_isHacked)
        {
            UnityEngine.Debug.Log("La puerta ya ha sido hackeada.");
            return;
        }

        _isHacked = true;

        // Aqu� ir�a la l�gica para abrir la puerta
        // Por ejemplo: animar la puerta, cambiar su collider, reproducir un sonido, etc.
        UnityEngine.Debug.Log("La puerta ha sido hackeada y abierta!");

        // Opcional: Desactivar el interactable para que no se pueda volver a usar
        GetComponent<HackerInteractable>().enabled = false;
    }
}
