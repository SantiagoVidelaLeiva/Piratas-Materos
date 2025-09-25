using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

// Este script es ahora un 'activador de color' genérico que puede ser llamado desde cualquier UnityEvent (como el de SingleUseSwitch o HackerInteractable).
public class SwitchColorChanger : MonoBehaviour
{

    [SerializeField] private ChangeColor _objectToColor;
    [SerializeField] private Color _colorToSet = Color.gray;


    // Este método es el nuevo punto de entrada. Lo hacemos público
    // para que pueda ser arrastrado y conectado en el Inspector de UnityEvents.
    public void ChangeObjectColor()
    {
        if (_objectToColor != null)
        {
            _objectToColor.SetColor(_colorToSet);
        }
        else
        {
            UnityEngine.Debug.LogError("Referencia a ChangeColor no encontrada en el Inspector de " + gameObject.name, this);
        }

        // Desactivar el componente después de un uso.
        // Puedes comentar esta línea si quieres que el objeto pueda cambiar de color varias veces.
        this.enabled = false;
    }
}
