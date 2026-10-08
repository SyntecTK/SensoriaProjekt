using UnityEngine;

public class HomieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject homiePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int homieCount = 7;

    private void Start()
    {
        SpawnHomies();
    }

    private void SpawnHomies()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        Transform[] shuffledSpawnPoints = (Transform[])spawnPoints.Clone();
        for (int i = shuffledSpawnPoints.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Transform temporary = shuffledSpawnPoints[i];
            shuffledSpawnPoints[i] = shuffledSpawnPoints[randomIndex];
            shuffledSpawnPoints[randomIndex] = temporary;
        }

        int spawnCount = Mathf.Min(homieCount, shuffledSpawnPoints.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = shuffledSpawnPoints[i];
            Instantiate(homiePrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
