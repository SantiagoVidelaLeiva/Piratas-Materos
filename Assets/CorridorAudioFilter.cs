using UnityEngine;
using UnityEngine.Audio;

public class CorridorAudioFilter : MonoBehaviour
{
    [Header("Mixer Snapshots")]
    private AudioMixerSnapshot explorationSnapshot;
    private AudioMixerSnapshot corridorSnapshot;
    [SerializeField] private AudioMixer audioMixer;
    private float transitionTime = 0.7f;

    private void Start()
    {
        var snapshotC = audioMixer.FindSnapshot("Corridor");
        corridorSnapshot = snapshotC;
        var snapshotE = audioMixer.FindSnapshot("Exploration");
        explorationSnapshot = snapshotE;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            corridorSnapshot.TransitionTo(transitionTime);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            explorationSnapshot.TransitionTo(transitionTime);
    }
}
