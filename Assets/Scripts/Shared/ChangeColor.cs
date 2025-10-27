using System.Diagnostics;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    private Renderer _renderer;
    // Usaremos Material[] para obtener instancias de los materiales y no modificar los assets originales.
    private Material[] _materialInstances;

    [Tooltip("Índice del material a modificar. 0 es el primero, 1 el segundo, etc.")]
    [SerializeField] private int _materialIndex = 0;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            UnityEngine.Debug.LogError("El componente Renderer no se encontró en este GameObject.", this);
            enabled = false;
            return;
        }

        _materialInstances = _renderer.materials;

        // Comprobación inicial
        if (_materialIndex < 0 || _materialIndex >= _materialInstances.Length)
        {
            UnityEngine.Debug.LogError($"El índice de material configurado ({_materialIndex}) está fuera del rango de materiales disponibles (0 a {_materialInstances.Length - 1}).", this);
        }
    }


    public void SetColor(Color newColor)
    {
        // Solo proceder si el índice es válido y las instancias existen
        if (_materialInstances != null && _materialIndex >= 0 && _materialIndex < _materialInstances.Length)
        {
            Material targetMaterial = _materialInstances[_materialIndex];

            targetMaterial.color = newColor;
        }
        else
        {
            UnityEngine.Debug.LogError("No se pudo aplicar el color. Revisa que el Material Index sea válido.", this);
        }
    }
}