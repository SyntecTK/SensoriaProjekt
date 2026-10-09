using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private const string SfxVolumeParam = "SFXVolume";
    private const float MutedVolume = -80f;

    [SerializeField] private AudioMixer gameMixer;

    private void OnEnable()
    {
        EventManager.OnIntroEnded += StartBGM;
        EventManager.OnRoundStarted += UnmuteSFX;
        EventManager.OnGameEnd += MuteSFX;
    }

    private void OnDisable()
    {
        EventManager.OnIntroEnded -= StartBGM;
        EventManager.OnRoundStarted -= UnmuteSFX;
        EventManager.OnGameEnd -= MuteSFX;
    }

    private void OnDestroy()
    {
        // Mixer-Werte bleiben über Szenenwechsel erhalten, daher beim Verlassen zurücksetzen
        UnmuteSFX();
    }

    private void StartBGM()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void MuteSFX()
    {
        if (gameMixer != null)
        {
            gameMixer.SetFloat(SfxVolumeParam, MutedVolume);
        }
    }

    private void UnmuteSFX()
    {
        if (gameMixer != null)
        {
            gameMixer.ClearFloat(SfxVolumeParam);
        }
    }
}
