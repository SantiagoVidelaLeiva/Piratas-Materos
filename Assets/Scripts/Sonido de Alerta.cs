using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAlertAudio : MonoBehaviour
{
    [SerializeField] private AudioClip alertClip;

    private AudioSource audioSource;
    private EnemyControllerBase enemy;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        enemy = GetComponent<EnemyControllerBase>();

        if (enemy != null)
        {
            enemy.OnStateChange += HandleStateChange;
        }
    }

    private void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.OnStateChange -= HandleStateChange;
        }
    }

    private void HandleStateChange(EnemyControllerBase.EnemyState newState)
    {
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