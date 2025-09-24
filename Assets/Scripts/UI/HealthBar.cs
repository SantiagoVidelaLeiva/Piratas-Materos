using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

// NOTA: Se ha eliminado la orden de ejecuci�n por defecto para
// permitir que este script se inicialice en el orden normal.
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    // La referencia al componente Health del jugador
    private Health _health;

    void Start()
    {
        // CAMBIO: Ya no buscamos el jugador a trav�s del GameManager.
        // En su lugar, lo buscamos directamente en la escena con su tag.
        // Aseg�rate de que tu jugador tenga el tag "Player".
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _health = player.GetComponent<Health>();
        }
        else
        {
            UnityEngine.Debug.LogError("No se encontr� un GameObject con el tag 'Player'. La barra de vida no funcionar� correctamente.");
            return; // Salimos de la funci�n si no encontramos al jugador.
        }

        // Si el slider no est� asignado en el Inspector, lo obtenemos del mismo GameObject.
        if (_slider == null)
        {
            _slider = GetComponent<Slider>();
        }

        // Verificamos que ambos componentes existan antes de continuar
        if (_health != null && _slider != null)
        {
            // Suscribimos el m�todo UpdateHealth al evento de cambio de vida del jugador.
            _health.OnHealthChanged += UpdateHealth;

            // Inicializamos la barra de vida con la vida m�xima.
            SetMaxHealth(_health.MaxHealth);
        }
        else
        {
            UnityEngine.Debug.LogError("Faltan componentes. Aseg�rate de que el GameObject tenga un Slider y de que el Player tenga el script Health.");
        }
    }

    // Establece el valor m�ximo de la barra de vida y el valor inicial.
    /// <param name="maxHealth">La vida m�xima.</param>
    public void SetMaxHealth(float maxHealth)
    {
        _slider.maxValue = maxHealth;
        _slider.value = maxHealth;
    }

    // Actualiza el valor actual de la barra de vida.
    /// <param name="health">La vida actual.</param>
    public void UpdateHealth(float health)
    {
        _slider.value = health;
    }
}