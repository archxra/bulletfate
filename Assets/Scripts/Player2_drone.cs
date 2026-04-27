using UnityEngine;

public class DroneAI : MonoBehaviour
{
    public float followSpeed = 5f;
    public float fireRate = 0.3f; // кд из дока
    public float damage = 2f;    // урон из дока
    public GameObject bulletPrefab;

    private Transform player;
    private float nextFire;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        // 1. Летим за игроком (держимся чуть в стороне)
        Vector3 targetPos = player.position + new Vector3(1.2f, 1.2f, 0);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // 2. Ищем ближайшего врага
        GameObject target = FindClosestEnemy();
        if (target != null)
        {
            // Крутимся к врагу
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // 3. Стреляем сами
            if (Time.time >= nextFire)
            {
                GameObject b = Instantiate(bulletPrefab, transform.position, transform.rotation);
                b.GetComponent<Bullet2D>().damage = damage;
                nextFire = Time.time + fireRate;
            }
        }
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float distance = 10f; // Радиус видимости дрона

        foreach (GameObject e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < distance) { closest = e; distance = d; }
        }
        return closest;
    }
}