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
    [SerializeField] private float stickerSizeMultiplier = 1.5f;
    [SerializeField, Range(0f, 1f)] private float stickerMirrorChance = 0.5f;

    [Header("Screens")]
    [SerializeField] private GameObject gameEndScreen;
    [SerializeField] private TMP_Text resultTXT;
    [SerializeField] private Image[] endStickerImages;
    [SerializeField] private Sprite[] endStickerSprites;
    [SerializeField] private float endStickerSizeMultiplier = 1f;
    [SerializeField] private float endStickerStartDelay = 2f;
    [SerializeField] private float endStickerInterval = 0.3f;

    private Vector3 originalScale;
    private Coroutine introCoroutine;
    private Coroutine stickerCoroutine;
    private Vector3 stickerOriginalCanvasPos;

    void Awake()
    {
        originalScale = introImage.transform.localScale;
        RectTransform canvasRect = (RectTransform)stickerImage.canvas.rootCanvas.transform;
        stickerOriginalCanvasPos = canvasRect.InverseTransformPoint(stickerImage.transform.position);
        audioSource = GetComponent<AudioSource>();
        gameEndScreen.SetActive(false);
    }

    void OnEnable()
    {
        EventManager.OnCollectibleCollected += UpdateUI;
        EventManager.OnCollectibleCollected += StickerPopUp;
        EventManager.OnRoundStarted += StartIntro;
        EventManager.OnGameEnd += HandleGameEnd;
    }
    
    void OnDisable()
    {
        EventManager.OnCollectibleCollected -= UpdateUI;
        EventManager.OnCollectibleCollected -= StickerPopUp;
        EventManager.OnRoundStarted -= StartIntro;
        EventManager.OnGameEnd -= HandleGameEnd;
    }

    void Update()
    {
        UpdateUI();
    }
    
    private void HandleGameEnd()
    {
        gameEndScreen.SetActive(true);
        resultTXT.text = $"You saved {GameManager.Instance.CurrentCollectibles} homies!";

        if (stickerCoroutine != null)
        {
            StopCoroutine(stickerCoroutine);
            stickerCoroutine = null;
        }
        stickerImage.gameObject.SetActive(false);

        if (endStickerSprites != null && endStickerSprites.Length > 0)
        {
            StartCoroutine(AnimateEndStickers());
        }
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
        const float holdDuration = 0.6f;
        const float fadeDuration = 1.2f;

        // Zufällig an der Canvas-Mitte horizontal spiegeln (nur X ändert sich)
        RectTransform canvasRect = (RectTransform)stickerImage.canvas.rootCanvas.transform;
        Vector3 canvasPos = stickerOriginalCanvasPos;
        if (Random.value < stickerMirrorChance)
        {
            canvasPos.x = 2f * canvasRect.rect.center.x - canvasPos.x;
        }
        stickerImage.transform.position = canvasRect.TransformPoint(canvasPos);

        yield return SlapSticker(stickerImage, sprite, stickerSizeMultiplier);

        yield return new WaitForSeconds(holdDuration);

        for (float e = 0f; e < fadeDuration; e += Time.deltaTime)
        {
            SetStickerAlpha(stickerImage, 1f - e / fadeDuration);
            yield return null;
        }

        stickerImage.gameObject.SetActive(false);
        stickerCoroutine = null;
    }

    private IEnumerator AnimateEndStickers()
    {
        foreach (Image image in endStickerImages)
        {
            image.gameObject.SetActive(false);
        }

        // Sprites mischen, damit jedes nur einmal vorkommt
        Sprite[] shuffled = (Sprite[])endStickerSprites.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        yield return new WaitForSeconds(endStickerStartDelay);

        int count = Mathf.Min(endStickerImages.Length, shuffled.Length);
        for (int i = 0; i < count; i++)
        {
            yield return SlapSticker(endStickerImages[i], shuffled[i], endStickerSizeMultiplier);
            yield return new WaitForSeconds(endStickerInterval);
        }
    }

    private IEnumerator SlapSticker(Image image, Sprite sprite, float sizeMultiplier)
    {
        const float slapDuration = 0.12f;
        const float settleDuration = 0.08f;
        float startScale = 0.3f * sizeMultiplier;
        float peakScale = 1.15f * sizeMultiplier;
        float endScale = 1f * sizeMultiplier;

        image.sprite = sprite;
        image.SetNativeSize();
        image.gameObject.SetActive(true);

        Transform t = image.transform;
        t.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f));
        SetStickerAlpha(image, 1f);

        // Schneller Aufschlag: klein -> leicht übergroß
        for (float e = 0f; e < slapDuration; e += Time.deltaTime)
        {
            float k = e / slapDuration;
            t.localScale = Vector3.one * Mathf.Lerp(startScale, peakScale, k * k);
            yield return null;
        }

        // Kurzes Zurückfedern auf Endgröße (Aufkleben)
        for (float e = 0f; e < settleDuration; e += Time.deltaTime)
        {
            t.localScale = Vector3.one * Mathf.Lerp(peakScale, endScale, e / settleDuration);
            yield return null;
        }
        t.localScale = Vector3.one * endScale;
    }

    private void SetStickerAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
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
        float duration = 0.7f;
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
