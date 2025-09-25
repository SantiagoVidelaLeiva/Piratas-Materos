using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAlertAudio : MonoBehaviour
{
    [SerializeField] private AudioClip alertClip;

    private AudioSource audioSource;
    private EnemyStateAggregator aggregator; // Referencia al Aggregator

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Busca la única instancia del Aggregator en la escena.
        aggregator = FindObjectOfType<EnemyStateAggregator>();

        if (aggregator != null)
        {
            // Ahora nos suscribimos al evento GLOBAL.
            aggregator.OnGlobalStateChange += HandleGlobalStateChange;
        }
    }

    private void OnDestroy()
    {
        // Limpiamos la suscripcin cuando el objeto se destruye.
        if (aggregator != null)
        {
            aggregator.OnGlobalStateChange -= HandleGlobalStateChange;
        }
    }

    private void HandleGlobalStateChange(EnemyControllerBase.EnemyState newState)
    {
        // Solo reproducimos el sonido si el estado global cambia a Danger.
        // Esto garantiza que solo suene una vez por amenaza.
        if (newState == EnemyControllerBase.EnemyState.Danger)
        {
            PlayAlertSound();
        }
    }

    private void PlayAlertSound()
    {
        if (alertClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(alertClip);
        }
    }
}