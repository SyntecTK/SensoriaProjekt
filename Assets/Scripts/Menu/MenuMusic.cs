using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] float targetVolume = 0.4f;
    [SerializeField] float fadeInTime = 1.5f;

    AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f;  
        src.volume = 0f;
    }

    void Start()
    {
        src.Play();
        StartCoroutine(Fade(targetVolume, fadeInTime));
    }

    public IEnumerator Fade(float to, float time)
    {
        float from = src.volume, t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        src.volume = to;
    }
}