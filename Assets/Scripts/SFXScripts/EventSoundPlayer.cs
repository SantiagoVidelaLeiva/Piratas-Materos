using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events; // Aunque no se usa directamente, es útil para recordar su contexto.

// Script simple que se conecta a un UnityEvent y reproduce un sonido.
[RequireComponent(typeof(AudioSource))]
public class EventSoundPlayer : MonoBehaviour
{
    // Campo para el clip que se reproducirá.
    [Tooltip("El clip de audio que se reproducirá una vez que se llame a la función PlaySound().")]
    [SerializeField] private AudioClip soundClip;

    private AudioSource audioSource;

    private void Awake()
    {
        // Obtiene el AudioSource que es obligatorio gracias a [RequireComponent].
        audioSource = GetComponent<AudioSource>();

        if (audioSource.playOnAwake)
        {
            UnityEngine.Debug.LogWarning("Se recomienda desactivar 'Play On Awake' en el AudioSource para que solo suene al activarse el interruptor.", this);
        }
    }

    public void PlaySound()
    {
        if (soundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundClip);
        }
        else
        {
            UnityEngine.Debug.LogWarning("No se pudo reproducir el sonido: falta el AudioClip o el AudioSource.", this);
        }
    }
}
