using System.Diagnostics;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    private Renderer _renderer;

    private void Awake()
    {
        // Obtiene el componente Renderer del objeto.
        _renderer = GetComponent<Renderer>();
    }

    /// <param name="newColor">El nuevo color para el objeto.</param>
    public void SetColor(Color newColor)
    {
        if (_renderer != null)
        {
            // Crea una instancia unica del material y cambia su color.
            _renderer.material.color = newColor;
        }
        else
        {
            UnityEngine.Debug.LogError("Renderer component not found on this GameObject.", this);
        }
    }
}