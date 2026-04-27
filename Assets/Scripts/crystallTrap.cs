using UnityEngine;
using System.Collections;

public class Golem_TrapLogic : MonoBehaviour
{
    [Tooltip("Сколько секунд есть у игрока, чтобы выбежать")]
    public float delayBeforeSnap = 2f;
    [Tooltip("Радиус схлопывания")]
    public float trapRadius = 1.5f;
    [Tooltip("Огромный урон от кристаллов")]
    public float trapDamage = 15f;

    void Start()
    {
        StartCoroutine(SnapRoutine());
    }

    IEnumerator SnapRoutine()
    {
        // Ждем (Игрок видит красный круг и убегает)
        yield return new WaitForSeconds(delayBeforeSnap);

        // СХЛОПЫВАНИЕ! Наносим урон
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, trapRadius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                h.GetComponent<Health>()?.TakeDamage(trapDamage);
                Debug.Log("<color=red>ИГРОК ПОПАЛСЯ В КРИСТАЛЛЫ!</color>");
            }
        }

        // Тут можно добавить префаб взрыва
        // Instantiate(explosionFx, transform.position, Quaternion.identity);

        Destroy(gameObject); // Уничтожаем ловушку
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, trapRadius);
    }
}