using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
    [System.Serializable]
    public class SpawnSettings
    {
        public GameObject enemyPrefab; // ˜˜˜˜ ˜˜˜˜˜˜˜
        public Transform spawnPoint;   // ˜˜˜ ˜˜˜˜˜˜˜
    }

    [Header("˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜")]
    public SpawnSettings[] enemiesToSpawn; // ˜˜˜˜˜˜: ˜˜˜˜ "˜˜˜˜ + ˜˜˜˜˜"

    [Header("˜˜˜˜˜")]
    public GameObject[] doors;

    private bool isRoomActive = false;
    private bool isFinished = false;
    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Start()
    {
        foreach (GameObject door in doors) door.SetActive(false);
        SetDoorVisualVisible(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isRoomActive && !isFinished)
        {
            StartRoom();
        }
    }

    void StartRoom()
    {
        isRoomActive = true;

        // 1. ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜
        foreach (GameObject door in doors) door.SetActive(true);
        SetDoorVisualVisible(false);

        // 2. ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        foreach (SpawnSettings setup in enemiesToSpawn)
        {
            if (setup.enemyPrefab != null && setup.spawnPoint != null)
            {
                GameObject enemy = Instantiate(setup.enemyPrefab, setup.spawnPoint.position, Quaternion.identity);
                aliveEnemies.Add(enemy);
            }
        }

        StartCoroutine(CheckEnemiesRoutine());
    }

    IEnumerator CheckEnemiesRoutine()
    {
        while (isRoomActive)
        {
            yield return new WaitForSeconds(0.5f);
            aliveEnemies.RemoveAll(item => item == null);

            if (aliveEnemies.Count == 0)
            {
                EndRoom();
            }
        }
    }

    void EndRoom()
    {
        isRoomActive = false;
        isFinished = true;
        foreach (GameObject door in doors) door.SetActive(false);
    }

    private void SetDoorVisualVisible(bool isVisible)
    {
        foreach (GameObject door in doors)
        {
            if (door == null) continue;
            SpriteRenderer[] renderers = door.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.enabled = isVisible;
            }
        }
    }
}