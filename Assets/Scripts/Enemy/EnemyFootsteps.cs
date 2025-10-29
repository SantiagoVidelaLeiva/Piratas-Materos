using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFootsteps : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioClip[] footstepClips;
    public float stepDistance = 2f;  // metros entre pasos
    public float minSpeedToPlay = 0.2f; // evita reproducir pasos cuando apenas se mueve

    [Header("Pitch & Volume Variance")]
    [Range(0.8f, 1.2f)] public float pitchVariance = 0.1f;
    [Range(0.8f, 1.2f)] public float volumeVariance = 0.1f;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Vector3 lastPos;
    private float distanceMoved;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        // Configurar audio como 3D espacial
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        lastPos = transform.position;
    }

    void Update()
    {
        // Calcula distancia recorrida
        float delta = Vector3.Distance(transform.position, lastPos);
        lastPos = transform.position;

        // Se acumula si se mueve lo suficiente
        if (agent.velocity.magnitude > minSpeedToPlay)
        {
            distanceMoved += delta;
            if (distanceMoved >= stepDistance)
            {
                PlayFootstep();
                distanceMoved = 0f;
            }
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
        audioSource.volume = 1f + Random.Range(-volumeVariance, volumeVariance);
        audioSource.PlayOneShot(clip);
    }
}