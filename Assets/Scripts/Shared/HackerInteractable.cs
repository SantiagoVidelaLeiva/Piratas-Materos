//using UnityEngine;

// Este script es el "interactable" remoto que se asocia con una cámara.
//public class HackerInteractable : MonoBehaviour, IInteractable
//{
//[SerializeField] private MonoBehaviour _hackableObject;
//[SerializeField] private string _interactPrompt = "Hackear Objeto [E]";

using UnityEngine;
using UnityEngine.Events; // No olvides añadir esto

// Este script es ahora un "disparador de eventos" remoto que se asocia con una cámara.
public class HackerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string _interactPrompt = "Hackear Sistema [E]";

    // Reemplazamos la referencia única a IHackable con un UnityEvent público.
    // Esto nos permite conectar múltiples acciones desde el Inspector.
    [SerializeField] private UnityEvent onInteract;

    public string InteractPrompt => _interactPrompt;

    public bool Interact()
    {
        // Cuando se interactúa, simplemente invocamos todos los métodos
        // que han sido conectados al evento en el Inspector.
        onInteract?.Invoke();

        // Puedes decidir si este tipo de interacción es de un solo uso.
        // Si quieres que el prompt desaparezca y no se pueda volver a hackear, devuelve 'true'.
        // Si quieres que se pueda hackear múltiples veces, devuelve 'false'.
        return true; // Asumimos que es de un solo uso por ahora.
    }
}