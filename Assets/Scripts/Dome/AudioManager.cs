using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.OnIntroEnded += StartBGM;
    }

    private void OnDisable()
    {
        EventManager.OnIntroEnded -= StartBGM;
    }

    private void StartBGM()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
