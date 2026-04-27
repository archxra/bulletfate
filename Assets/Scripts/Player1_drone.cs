using UnityEngine;
using UnityEngine.InputSystem;

public class Skill_Ultra_Sidekicks : MonoBehaviour
{
    public GameObject sidekickPrefab; // Префаб нашего помощника
    public float cooldown = 90f;     // Кулдаун из дока
    public float offsetBehind = 1.5f; // Насколько далеко сзади спавнятся

    private float nextUseTime = 0f;

    void Update()
    {
        if (Keyboard.current == null) return;

        // Давай назначим Ульту на кнопку 'R'
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUseTime)
        {
            ActivateUltra();
            nextUseTime = Time.time + cooldown;
        }
    }

    void ActivateUltra()
    {
        Debug.Log("УЛЬТА АКТИВИРОВАНА!");

        // Спавним 4-х помощников в ряд позади игрока
        for (int i = 0; i < 4; i++)
        {
            // Считаем позицию сзади игрока (минус transform.up)
            // И немного смещаем их влево-вправо (i - 1.5f), чтобы они стояли в ряд
            Vector3 spawnPos = transform.position - (transform.up * offsetBehind) + (transform.right * (i - 1.5f));

            Instantiate(sidekickPrefab, spawnPos, transform.rotation);
        }
    }
}