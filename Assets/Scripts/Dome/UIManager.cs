using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    [Header("IntroSequence")]
    [SerializeField] private Image introImage;
    [SerializeField] private Sprite[] introSprites;

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text timerTXT;
    [SerializeField] private TMP_Text collectiblesTXT;

    private Vector3 originalScale;
    private Coroutine introCoroutine;

    void Awake()
    {
        originalScale = introImage.transform.localScale;
    }

    void OnEnable()
    {
        EventManager.OnCollectibleCollected += UpdateUI;
        EventManager.OnRoundStarted += StartIntro;
    }
    
    void OnDisable()
    {
        EventManager.OnCollectibleCollected -= UpdateUI;
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
        foreach (Sprite sprite in introSprites)
        {
            introImage.gameObject.SetActive(true);
            introImage.sprite = sprite;
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
