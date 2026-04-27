using UnityEngine;
using UnityEngine.InputSystem;

public class Skill_Ultra_DroneSpawn : MonoBehaviour
{
    public GameObject dronePrefab;
    public float duration = 15f; // из дока
    public float cooldown = 90f; // из дока
    private float nextUse;

    void Update()
    {
        // На кнопку "R"
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUse)
        {
            GameObject d = Instantiate(dronePrefab, transform.position, Quaternion.identity);
            Destroy(d, duration); // Дрон удалится сам через 15 сек
            nextUse = Time.time + cooldown;
            Debug.Log("ДРОН ВЫПУЩЕН!");
        }
    }
}