using System.Diagnostics;
using UnityEngine;

// Enum para definir si la puerta se mueve, rota o ambas cosas.
// En este caso, nos centraremos en Traslación o Rotación.
public enum DoorMovementType
{
    Translation, // Se desliza (cambia de posición)
    Rotation     // Gira (cambia de ángulo)
}

// Script centralizado para controlar el comportamiento de una puerta.
// Implementa la interfaz IHackable.
public class DoorController : MonoBehaviour, IHackable
{
    [Header("Configuración de la Puerta")]
    [Tooltip("El objeto hijo de la puerta que se moverá o rotará.")]
    [SerializeField] private Transform _doorObject;
    [SerializeField] private DoorMovementType _movementType = DoorMovementType.Translation;
    [SerializeField] private float _openSpeed = 1f;
    [SerializeField] private string _interactPrompt = "E to Hack";

    [Header("--- TRASLACIÓN (Movimiento) ---")]
    [Tooltip("La posición local final (offset) cuando la puerta está abierta. EJ: (0, 3, 0) para subir 3 unidades.")]
    [SerializeField] private Vector3 _openTranslationOffset;

    [Header("--- ROTACIÓN (Ángulo) ---")]
    [Tooltip("Los ángulos de Euler (eje, ángulo) de la rotación local final cuando la puerta está abierta. EJ: (0, 90, 0) para girar 90 grados en Y.")]
    [SerializeField] private Vector3 _openEulerRotation;

    // Estados internos y valores de cierre
    private bool _isMoving = false;
    private bool _isOpen = false;

    // Valores de posición/rotación cerradas (guardadas en Awake)
    private Vector3 _closedPosition;
    private Quaternion _closedRotation;

    // Propiedad de solo lectura para saber el estado de la puerta
    public bool IsOpen => _isOpen;

    // Propiedad requerida por IHackable
    public string InteractPrompt => _interactPrompt;

    private void Awake()
    {
        // Se establecen la posición y rotación de la puerta iniciales (cerradas)
        if (_doorObject != null)
        {
            _closedPosition = _doorObject.localPosition;
            _closedRotation = _doorObject.localRotation;
        }
    }

    private void Update()
    {
        // Lógica de movimiento/rotación suave de la puerta
        if (_isMoving)
        {
            bool hasReachedTarget = false;

            if (_movementType == DoorMovementType.Translation)
            {
                // ** LÓGICA DE TRASLACIÓN (Mover Posición) **
                Vector3 targetPosition = _isOpen ? (_closedPosition + _openTranslationOffset) : _closedPosition;
                _doorObject.localPosition = Vector3.MoveTowards(_doorObject.localPosition, targetPosition, _openSpeed * Time.deltaTime);

                // Comprobamos si la posición objetivo ha sido alcanzada
                if (_doorObject.localPosition == targetPosition)
                {
                    hasReachedTarget = true;
                }
            }
            else if (_movementType == DoorMovementType.Rotation)
            {
                // ** LÓGICA DE ROTACIÓN (Girar Ángulo) **
                // Convertimos el Vector3 de ángulos de Euler a un Quaternion para la rotación objetivo abierta.
                Quaternion targetRotation = _isOpen ? Quaternion.Euler(_openEulerRotation) : _closedRotation;

                // Nota: Se usa un multiplicador (100) para que la velocidad sea apropiada para rotación (grados por segundo).
                _doorObject.localRotation = Quaternion.RotateTowards(_doorObject.localRotation, targetRotation, _openSpeed * 100f * Time.deltaTime);

                // Comprobamos si la rotación objetivo ha sido alcanzada
                if (_doorObject.localRotation == targetRotation)
                {
                    hasReachedTarget = true;
                }
            }

            // Detenemos el movimiento/rotación cuando la puerta llega a su destino
            if (hasReachedTarget)
            {
                _isMoving = false;
            }
        }
    }

    // Abre la puerta. Este método puede ser llamado por cualquier otro script.
    public void OpenDoor()
    {
        if (!_isOpen && !_isMoving)
        {
            UnityEngine.Debug.Log("Door is now opening...");
            _isOpen = true;
            _isMoving = true;
        }
    }

    // Cierra la puerta. Puede ser útil para puertas que se cierran solas.
    public void CloseDoor()
    {
        if (_isOpen && !_isMoving)
        {
            UnityEngine.Debug.Log("Door is now closing...");
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
        // Alternamos entre abrir y cerrar al interactuar
        if (_isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
}