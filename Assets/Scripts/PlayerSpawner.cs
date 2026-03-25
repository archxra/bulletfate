using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private GameObject currentPlayer;

    void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (currentPlayer != null)
            Destroy(currentPlayer);

        currentPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity);
    }
}