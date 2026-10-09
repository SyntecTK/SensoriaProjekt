using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [Header("Game Settings")]
    [SerializeField] private int roundTimeInSeconds;
    [SerializeField] private int totalCollectibles;
    [SerializeField] private float resetDelay = 10f;

    private int currentCollectibles = 0;
    public int CurrentCollectibles => currentCollectibles;
    private float remainingTime;
    private readonly Dictionary<Rigidbody, bool> originalKinematicStates = new Dictionary<Rigidbody, bool>();

    private void OnEnable()
    {
        EventManager.OnIntroEnded += StartRoundTimer;
    }

    private void OnDisable()
    {
        EventManager.OnIntroEnded -= StartRoundTimer;
    }

    private void StartRoundTimer()
    {
        StartCoroutine(RoundTimer());
    }

    private void Start()
    {
        remainingTime = roundTimeInSeconds;
        EventManager.RoundStarted();
    }

    private void Update()
    {
        
    }

    //-------------------------------------------------------------------

    public void CollectibleCollected()
    {
        currentCollectibles++;
        EventManager.CollectibleCollected();
    }

    public float GetRemainingTime()
    {
        return Mathf.Max(0, remainingTime);
    }

    public void SetAllRigidbodiesKinematic(bool isKinematic)
    {
        Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>();
        foreach (var rb in rigidbodies)
        {
            // Nur den ersten Wert merken, damit mehrfaches Setzen den Ursprungswert nicht überschreibt
            if (!originalKinematicStates.ContainsKey(rb))
            {
                originalKinematicStates[rb] = rb.isKinematic;
            }
            rb.isKinematic = isKinematic;
        }
    }

    public void ResetAllRigidbodyKinematics()
    {
        foreach (var entry in originalKinematicStates)
        {
            if (entry.Key != null)
            {
                entry.Key.isKinematic = entry.Value;
            }
        }
        originalKinematicStates.Clear();
    }

    IEnumerator RoundTimer()
    {
        remainingTime = roundTimeInSeconds;

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        EventManager.GameEnded();
        StartCoroutine(ResetGame());
    }
    IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(resetDelay);
        SceneManager.LoadScene("MainMenu");
    }
}
