using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Soldier_AirStrike : MonoBehaviour
{
    public float damage = 50f;
    public float cooldown = 150f;
    public GameObject screenFlash; // Необязательно: красный квадрат на весь экран

    private float nextUse;

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUse)
        {
            StartCoroutine(CallAirstrike());
            nextUse = Time.time + cooldown;
        }
    }

    IEnumerator CallAirstrike()
    {
        Debug.Log("ВЫЗЫВАЮ ПОДДЕРЖКУ С ВОЗДУХА!");

        // Предупреждение (мигаем 3 раза)
        for (int i = 0; i < 3; i++)
        {
            if (screenFlash != null) screenFlash.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            if (screenFlash != null) screenFlash.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }

        // БАБАХ по всем врагам на карте
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Health h = enemy.GetComponent<Health>();
            if (h != null) h.TakeDamage(damage);
        }

        Debug.Log("ЦЕЛИ УНИЧТОЖЕНЫ");
    }
}