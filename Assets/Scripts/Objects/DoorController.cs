using UnityEngine;

// Script centralizado para controlar el comportamiento de una puerta.
// La l�gica de apertura y cierre se encuentra aqu�, independientemente de
// si es activada por un switch, un hack, etc.
public class DoorController : MonoBehaviour, IHackable
{
    [Header("Configuraci�n de la Puerta")]
    [SerializeField] private Transform _doorObject;
    [SerializeField] private Vector3 _openPosition;
    [SerializeField] private float _openSpeed = 1f;
    [SerializeField] private Vector3 _closedPosition;
    [SerializeField] private string _interactPrompt = "E to Hack";

    // Estado interno de la puerta
    private bool _isMoving = false;
    private bool _isOpen = false;

    // Propiedad de solo lectura para saber el estado de la puerta
    public bool IsOpen => _isOpen;

    // Propiedad requerida por IHackable
    public string InteractPrompt => _interactPrompt;

    private void Awake()
    {
        // Se establece la posici�n de la puerta en la posici�n inicial (cerrada)
        if (_doorObject != null)
        {
            _closedPosition = _doorObject.localPosition;
        }
    }

    private void Update()
    {
        // L�gica de movimiento suave de la puerta
        if (_isMoving)
        {
            Vector3 targetPosition = _isOpen ? _openPosition : _closedPosition;
            _doorObject.localPosition = Vector3.MoveTowards(_doorObject.localPosition, targetPosition, _openSpeed * Time.deltaTime);

            // Detenemos el movimiento cuando la puerta llega a su destino
            if (_doorObject.localPosition == targetPosition)
            {
                _isMoving = false;
            }
        }
    }

    // Abre la puerta. Este m�todo puede ser llamado por cualquier otro script.
    public void OpenDoor()
    {
        if (!_isOpen && !_isMoving)
        {
            Debug.Log("Door is now opening...");
            _isOpen = true;
            _isMoving = true;
        }
    }


    // Cierra la puerta. Puede ser �til para puertas que se cierran solas.
    public void CloseDoor()
    {
        if (_isOpen && !_isMoving)
        {
            Debug.Log("Door is now closing...");
            _isOpen = false;
            _isMoving = true;
        }
    }

    public bool Interact()
    {
        Hack();
        return true;
    }

    public void Hack()
    {
        OpenDoor();
    }
}