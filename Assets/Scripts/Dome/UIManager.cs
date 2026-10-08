using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    [Header("IntroSequence")]
    [SerializeField] private Image introImage;
    [SerializeField] private Sprite[] introSprites;
    private AudioSource audioSource;
    

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text timerTXT;
    [SerializeField] private TMP_Text collectiblesTXT;

    [Header("Gameplay Stickers")]
    [SerializeField] private Image stickerImage;
    [SerializeField] private Sprite[] stickerSprites;

    private Vector3 originalScale;
    private Coroutine introCoroutine;
    private Coroutine stickerCoroutine;

    void Awake()
    {
        originalScale = introImage.transform.localScale;
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        EventManager.OnCollectibleCollected += UpdateUI;
        EventManager.OnCollectibleCollected += StickerPopUp;
        EventManager.OnRoundStarted += StartIntro;
    }
    
    void OnDisable()
    {
        EventManager.OnCollectibleCollected -= UpdateUI;
        EventManager.OnCollectibleCollected -= StickerPopUp;
        EventManager.OnRoundStarted -= StartIntro;
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        int remainingSeconds = Mathf.CeilToInt(GameManager.Instance.GetRemainingTime());
        timerTXT.text = $"{remainingSeconds / 60:00}:{remainingSeconds % 60:00}";
        collectiblesTXT.text = GameManager.Instance.CurrentCollectibles.ToString("D3");
    }

    private void StickerPopUp()
    {
        if (stickerSprites == null || stickerSprites.Length == 0) return;

        if (stickerCoroutine != null)
        {
            StopCoroutine(stickerCoroutine);
        }
        stickerCoroutine = StartCoroutine(AnimateSticker(stickerSprites[Random.Range(0, stickerSprites.Length)]));
    }

    private IEnumerator AnimateSticker(Sprite sprite)
    {
        const float slapDuration = 0.12f;
        const float holdDuration = 0.6f;
        const float fadeDuration = 1.2f;
        const float startScale = 0.3f;
        const float peakScale = 1.15f;
        const float endScale = 1f;

        stickerImage.sprite = sprite;
        stickerImage.SetNativeSize();
        stickerImage.gameObject.SetActive(true);

        Transform t = stickerImage.transform;
        t.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f));
        SetStickerAlpha(1f);

        // Schneller Aufschlag: klein -> leicht übergroß
        for (float e = 0f; e < slapDuration; e += Time.deltaTime)
        {
            float k = e / slapDuration;
            t.localScale = Vector3.one * Mathf.Lerp(startScale, peakScale, k * k);
            yield return null;
        }

        // Kurzes Zurückfedern auf Endgröße (Aufkleben)
        const float settleDuration = 0.08f;
        for (float e = 0f; e < settleDuration; e += Time.deltaTime)
        {
            t.localScale = Vector3.one * Mathf.Lerp(peakScale, endScale, e / settleDuration);
            yield return null;
        }
        t.localScale = Vector3.one * endScale;

        yield return new WaitForSeconds(holdDuration);

        for (float e = 0f; e < fadeDuration; e += Time.deltaTime)
        {
            SetStickerAlpha(1f - e / fadeDuration);
            yield return null;
        }

        stickerImage.gameObject.SetActive(false);
        stickerCoroutine = null;
    }

    private void SetStickerAlpha(float alpha)
    {
        Color c = stickerImage.color;
        c.a = alpha;
        stickerImage.color = c;
    }
    
    private void StartIntro()
    {
        if (introCoroutine != null)
        {
            StopCoroutine(introCoroutine);
        }
        introCoroutine = StartCoroutine(AnimateIntroImages());
    }

    private IEnumerator AnimateIntroImages()
    {
        audioSource.Play();
        
        foreach (Sprite sprite in introSprites)
        {
            introImage.gameObject.SetActive(true);
            introImage.sprite = sprite;
            introImage.SetNativeSize();
            yield return AnimateIntroImage();
        }

        introImage.gameObject.SetActive(false);
        introImage.transform.localScale = originalScale;
        introCoroutine = null;

        EventManager.IntroEnded();
    }

    private IEnumerator AnimateIntroImage()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            introImage.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        introImage.transform.localScale = originalScale * 1.2f;
    }
}
