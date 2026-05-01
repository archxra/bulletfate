using UnityEngine;
using UnityEngine.UI; // Обязательно для работы с текстом

public class UI_HealthDisplay : MonoBehaviour
{
    public Text hpText; // Сюда перетащи объект текста в инспекторе
    private Health currentPlayerHealth;

    void Update()
    {
        if (hpText != null)
        {
            hpText.color = Color.red;
        }

        // Ищем активного игрока и его скрипт Health
        if (currentPlayerHealth == null || !currentPlayerHealth.gameObject.activeInHierarchy)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                currentPlayerHealth = player.GetComponent<Health>();
            }
        }

        // Если нашли игрока - обновляем текст
        if (currentPlayerHealth != null)
        {
            hpText.text = "HEALTH: " + currentPlayerHealth.currentHealth + " / " + currentPlayerHealth.maxHealth;
        }
        else
        {
            hpText.text = "HEALTH: --";
        }
    }
}