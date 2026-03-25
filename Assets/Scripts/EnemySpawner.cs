using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxEnemies = 10;

    private int currentEnemies = 0;

    void Start()
    {
        SpawnAll();
    }

    void SpawnAll()
    {
        foreach (Transform point in spawnPoints)
        {
            if (currentEnemies >= maxEnemies) break;
            SpawnAt(point.position);
        }
    }

    void SpawnAt(Vector3 position)
    {
        int index = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[index], position, Quaternion.identity);
        currentEnemies++;
    }

    public void OnEnemyDied()
    {
        currentEnemies--;
    }
}