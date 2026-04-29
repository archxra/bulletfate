using UnityEngine;
using UnityEngine.InputSystem;

public class Skill_FernandoUltra : MonoBehaviour
{
    public GameObject ultraPrefab; // Префаб Fernando_Ultra_Visual
    public float cooldown = 90f;
    private float nextUse;

    void Update()
    {
        // Ульта на кнопку R
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUse)
        {
            Activate();
            nextUse = Time.time + cooldown;
        }
    }

    void Activate()
    {
        // Создаем ульту в позиции игрока
        // Она сама подождет секунду, выстрелит и удалится
        Instantiate(ultraPrefab, transform.position, Quaternion.identity);
    }
}