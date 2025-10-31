using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

public class RandomPlay : MonoBehaviour
{

    [SerializeField] private AudioClip[] clips;
    private AudioSource audioSource;
    private float timer;
    private float interval;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        timer += Time.deltaTime;
        
        if(timer >= interval)
        {
            PlayRandomClip();
            timer = 0;
            interval = Random.Range(0, 6);
        }
            
    }

    void PlayRandomClip()
    {
        if (clips.Length == 0) return;
        int index = Random.Range(0, clips.Length);
        audioSource.PlayOneShot(clips[index]);
    }
}
