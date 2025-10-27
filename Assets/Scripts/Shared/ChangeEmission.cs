using UnityEngine;

// Este script maneja únicamente el color de Emisión (brillo) de un material.
public class ChangeEmission : MonoBehaviour
{
    private Renderer _renderer;

    [SerializeField] private int _materialIndex = 0;

    private Material[] _materialInstances;

    // Constante que define el nombre de la propiedad de Emisión en los Shaders estándar de Unity.
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    // Palabra clave necesaria para que la emisión funcione en muchos shaders (como el Standard).
    private const string EmissionKeyword = "_EMISSION";

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            // Creamos una instancia única del material. Esto es VITAL para que los cambios solo afecten a este objeto y no a otros que usen el mismo material.
            _materialInstances = _renderer.materials;
        }
        else
        {
            UnityEngine.Debug.LogError("El componente Renderer no se encontró en este GameObject.", this);
            enabled = false;
        }
    }

    public void SetEmission(Color emissionColor, float intensity)
    {
        if (_renderer == null || _materialInstances == null) return;

        if (_materialIndex < 0 || _materialIndex >= _materialInstances.Length)
        {
            UnityEngine.Debug.LogError($"Índice de material ({_materialIndex}) fuera de rango en {gameObject.name}. Materiales disponibles: {_materialInstances.Length}", this);
            return;
        }

        Material targetMaterial = _materialInstances[_materialIndex];

        Color finalEmissionColor = emissionColor * intensity;

        // Aplica el color de Emisión al shader.
        targetMaterial.SetColor(EmissionColorID, finalEmissionColor);

        // Habilita o deshabilita la palabra clave de Emisión del shader.
        if (intensity > 0.01f)
        {
            targetMaterial.EnableKeyword(EmissionKeyword);
        }
        else
        {
            targetMaterial.DisableKeyword(EmissionKeyword);
        }
    }

    public void TurnOffEmission()
    {
        // Apaga la emisión con intensidad cero.
        SetEmission(Color.black, 0f);
    }

    public void TurnOnEmission(Color color, float intensity = 3.0f)
    {
        SetEmission(color, intensity);
    }
}