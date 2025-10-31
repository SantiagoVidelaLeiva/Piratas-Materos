using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class MusicFader : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string parameterName = "MusicExploration";
    [SerializeField] private float fadeDuration = 3f;
    [SerializeField] private float targetDb = 0f; 
    private float minDb = -50f; 

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            float currentDb = Mathf.Lerp(minDb, targetDb, t);
            mixer.SetFloat(parameterName, currentDb);
            yield return null;
        }
        mixer.SetFloat(parameterName, targetDb);
    }
}