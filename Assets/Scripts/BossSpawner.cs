using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab;

    void Start()
    {
        Instantiate(bossPrefab, transform.position, Quaternion.identity);
    }
}