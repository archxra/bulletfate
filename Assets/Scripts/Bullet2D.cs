using UnityEngine;
public class Bullet2D : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 4f;
    public float lifeTime = 2f;
    [Header("Кого бьем?")]
    public string targetTag = "Enemy";
    [Header("Стены")]
    public LayerMask obstacleLayer;
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Health>()?.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}