using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Skill_Ultra_Chariot : MonoBehaviour
{
    public float duration = 15f;     // 15 сек по документу
    public float speedMultiplier = 2f; // скорость х2 по документу
    public float cooldown = 120f;    // 120 сек кд по документу

    private PlayerController2D moveScript;
    private Health healthScript;
    private float nextUse;

    void Start()
    {
        moveScript = GetComponent<PlayerController2D>();
        healthScript = GetComponent<Health>();
    }

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUse)
        {
            StartCoroutine(ChariotRoutine());
            nextUse = Time.time + cooldown;
        }
    }

    IEnumerator ChariotRoutine()
    {
        Debug.Log("КОЛЕСНИЦА!");
        float originalSpeed = moveScript.moveSpeed;

        // Включаем бессмертие и скорость
        healthScript.isInvincible = true;
        moveScript.moveSpeed *= speedMultiplier;

        // Визуально: кубик становится желтым, пока едет "на колеснице"
        GetComponent<SpriteRenderer>().color = Color.yellow;

        yield return new WaitForSeconds(duration);

        // Возвращаем всё как было
        moveScript.moveSpeed = originalSpeed;
        healthScript.isInvincible = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}