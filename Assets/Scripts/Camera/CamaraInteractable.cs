using UnityEngine;

// Esta clase es una estructura que empareja una Cámara de Seguridad con un objeto IInteractable.
// Se usa en el script SistemaDeCamaras para hacer el array más flexible.
[System.Serializable]
public class CameraInteractable
{
    // La cámara de seguridad en sí
    public Camera Camera;
    // El objeto IInteractable asociado a esta cámara
    public MonoBehaviour InteractableObject;
    // Referencia al objeto que implementa IInteractable
    private IInteractable _interactable;

    // Obtiene la referencia de la interfaz
    public IInteractable Interactable
    {
        get
        {
            if (_interactable == null && InteractableObject != null)
            {
                _interactable = InteractableObject.GetComponent<IInteractable>();
            }
            return _interactable;
        }
    }

    // Propiedad para el mensaje de interacción
    public string InteractPrompt
    {
        get
        {
            if (Interactable != null)
            {
                return Interactable.InteractPrompt;
            }
            return null;
        }
    }
}