//using UnityEngine;

// Este script es el "interactable" remoto que se asocia con una c�mara.
//public class HackerInteractable : MonoBehaviour, IInteractable
//{
//[SerializeField] private MonoBehaviour _hackableObject;
//[SerializeField] private string _interactPrompt = "Hackear Objeto [E]";

using UnityEngine;
using UnityEngine.Events; // No olvides a�adir esto

// Este script es ahora un "disparador de eventos" remoto que se asocia con una c�mara.
public class HackerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string _interactPrompt = "Hackear Sistema [E]";

    // Reemplazamos la referencia �nica a IHackable con un UnityEvent p�blico.
    // Esto nos permite conectar m�ltiples acciones desde el Inspector.
    [SerializeField] private UnityEvent onInteract;

    public string InteractPrompt => _interactPrompt;

    public bool Interact()
    {
        // Cuando se interact�a, simplemente invocamos todos los m�todos
        // que han sido conectados al evento en el Inspector.
        onInteract?.Invoke();
        _interactPrompt = "";

        // Puedes decidir si este tipo de interacci�n es de un solo uso.
        // Si quieres que el prompt desaparezca y no se pueda volver a hackear, devuelve 'true'.
        // Si quieres que se pueda hackear m�ltiples veces, devuelve 'false'.
        return true; // Asumimos que es de un solo uso por ahora.
    }
}