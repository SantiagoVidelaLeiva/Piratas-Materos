using UnityEngine;
using System.Diagnostics; // Solo necesario si usas el Debug.LogError de System.Diagnostics

// Este script actúa como un "nexo" o puente para activar la emisión.
// Permite que un UnityEvent (como el de HackerInteractable) llame a la función
// SetEmission con parámetros predefinidos desde el Inspector.
public class SwitchEmissionChanger : MonoBehaviour
{
    [Header("Referencia a la Pantalla/Objeto")]
    // Referencia al script que controla la emisión de la pantalla de TV
    [SerializeField] private ChangeEmission _objectToControl;

    [Header("Configuración de la Emisión")]
    [Tooltip("Color que emitirá la pantalla cuando se active.")]
    [SerializeField] private Color _emissionColor = Color.cyan;

    [Tooltip("Intensidad del brillo. 0 para apagado, 1+ para encendido.")]
    [Range(0f, 10f)] // Rango visual para mejor control en el inspector
    [SerializeField] private float _emissionIntensity = 4.0f;


    public void ChangeObjectEmission()
    {
        if (_objectToControl != null)
        {
            // Llama a la función de ChangeEmission usando los valores predefinidos.
            _objectToControl.SetEmission(_emissionColor, _emissionIntensity);
        }
        else
        {
            UnityEngine.Debug.LogError("Referencia a ChangeEmission no encontrada en el Inspector de " + gameObject.name, this);
        }

        // Opción: Desactivar el componente después del cambio. Si la interacción es un interruptor que cambia de estado, NO deshabilitar.
        // this.enabled = false;
    }

    public void TurnOffScreen()
    {
        if (_objectToControl != null)
        {
            _objectToControl.TurnOffEmission();
        }
    }
}
