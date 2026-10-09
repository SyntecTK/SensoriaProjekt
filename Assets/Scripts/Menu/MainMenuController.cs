using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] RectTransform mainPanel;
    [SerializeField] RectTransform settingsPanel;
    [SerializeField] RectTransform creditsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button quitButton;

    [Header("Zurück-Buttons")]
    [SerializeField] Button[] backButtons;

    [Header("Spiel")]
    [SerializeField] string gameSceneName = "GameScene";

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip clickSound;
    [SerializeField] MenuMusic music;

    [Header("Animation")]
    [SerializeField] Canvas canvas;
    [SerializeField] float moveDuration = 0.45f;
    [SerializeField] float stagger = 0.07f;

    readonly Dictionary<RectTransform, Vector2> homePos = new();
    readonly List<RectTransform> allPanels = new();
    CanvasGroup canvasGroup;
    RectTransform currentPanel;
    bool busy;

    void Awake()
    {
        canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();

        allPanels.AddRange(new[] { mainPanel, settingsPanel, creditsPanel });
        foreach (var p in allPanels)
        {
            if (p == null) continue;
            p.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();

            foreach (var lg in p.GetComponentsInChildren<LayoutGroup>(true))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(lg.GetComponent<RectTransform>());
                lg.enabled = false;
            }
            foreach (var csf in p.GetComponentsInChildren<ContentSizeFitter>(true)) csf.enabled = false;
            foreach (RectTransform child in p) homePos[child] = child.anchoredPosition;
            p.gameObject.SetActive(p == mainPanel);
        }
        currentPanel = mainPanel;
    }

    void OnEnable()
    {
        startButton.onClick.AddListener(OnStart);
        quitButton.onClick.AddListener(OnQuit);
        settingsButton.onClick.AddListener(() => SwitchTo(settingsPanel));
        creditsButton.onClick.AddListener(() => SwitchTo(creditsPanel));
        foreach (var b in backButtons) if (b) b.onClick.AddListener(() => SwitchTo(mainPanel));
    }

    void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        creditsButton.onClick.RemoveAllListeners();
        foreach (var b in backButtons) if (b) b.onClick.RemoveAllListeners();
    }

    void PlayClick()
    {
        if (audioSource && clickSound) audioSource.PlayOneShot(clickSound);
    }

    void OnStart()
    {
        if (busy) return;
        PlayClick();
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        busy = true;
        canvasGroup.interactable = false;
        if (music) StartCoroutine(music.Fade(0f, moveDuration));
        yield return Animate(currentPanel, false);
        SceneManager.LoadScene(gameSceneName);
    }

    void OnQuit()
    {
        if (busy) return;
        PlayClick();
        StartCoroutine(QuitRoutine());
    }

    IEnumerator QuitRoutine()
    {
        busy = true;
        yield return new WaitForSecondsRealtime(clickSound ? Mathf.Min(clickSound.length, 0.4f) : 0f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SwitchTo(RectTransform target)
    {
        if (busy || target == currentPanel) return;
        PlayClick();
        StartCoroutine(SwitchRoutine(target));
    }

    IEnumerator SwitchRoutine(RectTransform target)
    {
        busy = true;
        canvasGroup.interactable = false;

        yield return Animate(currentPanel, false);
        currentPanel.gameObject.SetActive(false);

        target.gameObject.SetActive(true);
        PlaceOffscreen(target);
        yield return Animate(target, true);

        currentPanel = target;
        canvasGroup.interactable = true;
        busy = false;
    }

    Vector2 OffscreenPos(RectTransform el, int index)
    {
        float canvasW = ((RectTransform)canvas.transform).rect.width;
        float side = index % 2 == 0 ? -1f : 1f;
        Vector2 home = homePos[el];
        return new Vector2(side * (canvasW * 0.5f + el.rect.width), home.y);
    }

    void PlaceOffscreen(RectTransform panel)
    {
        int i = 0;
        foreach (RectTransform el in panel) el.anchoredPosition = OffscreenPos(el, i++);
    }

    IEnumerator Animate(RectTransform panel, bool entering)
    {
        var els = new List<RectTransform>();
        foreach (RectTransform el in panel) els.Add(el);

        float total = moveDuration + stagger * (els.Count - 1);
        float t = 0f;
        while (t < total)
        {
            t += Time.unscaledDeltaTime;
            for (int i = 0; i < els.Count; i++)
            {
                float p = Mathf.Clamp01((t - i * stagger) / moveDuration);
                Vector2 home = homePos[els[i]];
                Vector2 off = OffscreenPos(els[i], i);
                els[i].anchoredPosition = entering
                    ? Vector2.LerpUnclamped(off, home, EaseOutCubic(p))
                    : Vector2.LerpUnclamped(home, off, EaseInBack(p));
            }
            yield return null;
        }
        for (int i = 0; i < els.Count; i++)
            els[i].anchoredPosition = entering ? homePos[els[i]] : OffscreenPos(els[i], i);
    }

    static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
    static float EaseInBack(float x) { const float c = 1.2f; return (c + 1f) * x * x * x - c * x * x; }
}