using UnityEngine;
using UnityEngine.AI; // Обязательно подключаем библиотеку ИИ!
using System.Collections;

public class EnemyAI_NavMesh : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // ВАЖНЫЕ НАСТРОЙКИ ДЛЯ 2D (чтобы спрайт не падал на спину и не крутился)
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        StartCoroutine(ThinkRoutine());
    }

    void Update()
    {
        // Ищем игрока
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Если игрок жив, даем Агенту команду: "Иди в точку, где стоит игрок"
        // Агент САМ построит маршрут, обойдет все стены и лабиринты!
        if (player != null && agent.isActiveAndEnabled)
        {
            agent.SetDestination(player.position);
        }
    }

    // Тот самый рваный темп (остановки)
    IEnumerator ThinkRoutine()
    {
        while (true)
        {
            agent.isStopped = false; // Бежит
            yield return new WaitForSeconds(Random.Range(1f, 2f));

            agent.isStopped = true;  // Резко тормозит
            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));
        }
    }
}