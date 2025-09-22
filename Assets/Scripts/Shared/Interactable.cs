using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private string _interactPrompt = "Press E to Interact"; // mensaje que aparace

    private IInteractable _interactableObject;  // Referencias
    private UIManager _uiManager;
    private bool _playerInRange = false;

    private void Awake()
    {
        _uiManager = FindObjectOfType<UIManager>();
        _interactableObject = GetComponent<IInteractable>();

        if (_interactableObject == null)
        {
            UnityEngine.Debug.LogError("Interactable.cs requiere que un componente IInteractable esté en el mismo GameObject.", this);
            enabled = false;
        }

        GetComponent<BoxCollider>().isTrigger = true; // Configura el BoxCollider como un trigger.
    }

    private void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))  // Revisa si el jugador esta en rango
        {
            bool interactionIsFinal = _interactableObject.Interact();
            
            if (interactionIsFinal)  // Si la interaccion es final
            {
                if (_uiManager != null)
                {
                    _uiManager.HideInteractPrompt();
                }

                enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {     
        if (other.CompareTag("Player"))   // Revisa si el objeto que entró en el trigger es el jugador.
        {
            _uiManager.ShowInteractPrompt(_interactPrompt);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))    // Revisa si el jugador está en el área y si presiona la tecla de interacción.
        {
            _interactableObject.Interact();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _uiManager.HideInteractPrompt();
        }
    }
}
