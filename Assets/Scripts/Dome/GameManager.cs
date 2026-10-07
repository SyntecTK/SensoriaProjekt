using System.Collections;
using UnityEngine;

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
    private int currentCollectibles = 0;
    public int CurrentCollectibles => currentCollectibles;
    private float remainingTime;

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

    IEnumerator RoundTimer()
    {
        remainingTime = roundTimeInSeconds;
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        // Round ended, handle end of round logic here
    }
}
