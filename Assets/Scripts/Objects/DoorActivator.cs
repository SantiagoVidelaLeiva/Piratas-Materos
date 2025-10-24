using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events; // Necesario para usar UnityEvent

// Este script controla una puerta que requiere múltiples activaciones.
public class DoorActivator : MonoBehaviour
{
    // Nueva variable para establecer el nmero de activaciones requeridas.
    // Esto hace que la puerta sea dinmica.
    [SerializeField] private int _requiredActivations = 1;

    [Header("Configuracion de la Puerta")]
    [SerializeField] private Transform _doorObject;
    [SerializeField] private Vector3 _openEulerRotation;
    [SerializeField] private float _openSpeed = 50f;

    private Quaternion _startRotation;
    private bool _isOpening = false;

    // Un contador de activaciones que es de instancia.
    private int _activatedSwitchesCount = 0;

    private void Awake()
    {
        if (_doorObject == null)
        {
            UnityEngine.Debug.LogError("Referencia a la puerta (_doorObject) no encontrada.", this);
            enabled = false;
            return;
        }

        _startRotation = _doorObject.localRotation;

        //  Eliminamos la suscripción en cdigo.
        // La conexin ahora se har en el Inspector.
        // foreach (SingleUseSwitch s in _requiredSwitches)
        // {
        //     s.OnActivated += OnSwitchActivated;
        // }
    }

    private void Update()
    {
        if (_isOpening)
        {
            Quaternion targetRotation = Quaternion.Euler(_openEulerRotation);

            _doorObject.localRotation = Quaternion.RotateTowards(_doorObject.localRotation, targetRotation, _openSpeed * Time.deltaTime);
            
            if (_doorObject.localRotation == targetRotation)
            {
                _isOpening = false;
            }
        }
    }

    //  Este es el mtodo que se conectar a los UnityEvents de los interruptores.
    public void OnSwitchActivated()
    {
        //  Verificamos si la puerta ya se est abriendo para evitar activaciones adicionales.
        if (_isOpening)
        {
            return;
        }
        _activatedSwitchesCount++;

        //  Ahora comparamos con la nueva variable dinmica.
        if (_activatedSwitchesCount >= _requiredActivations)
        {
            UnityEngine.Debug.Log("Puerta activada por todos los interruptores!");
            _isOpening = true;

            //  Eliminamos la desuscripcin de eventos.
            // Esto ya no es necesario porque no hay una suscripcin en el cdigo.
            // foreach (SingleUseSwitch s in _requiredSwitches)
            // {
            //    s.OnActivated -= OnSwitchActivated;
            // }
        }
    }

    //  Mtodo para reiniciar la puerta.
    public void ResetDoor()
    {
        _activatedSwitchesCount = 0;
        _isOpening = false;
        _doorObject.localRotation = _startRotation;
    }
}