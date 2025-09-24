using UnityEngine;

public interface IInteractable
{
    string InteractPrompt { get; }

    // Define el método que se ejecutará cuando el jugador interactúe.
    bool Interact();
}
