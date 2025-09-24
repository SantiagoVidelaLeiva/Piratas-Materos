using UnityEngine;

public interface IInteractable
{
    string InteractPrompt { get; }

    // Define el m�todo que se ejecutar� cuando el jugador interact�e.
    bool Interact();
}
