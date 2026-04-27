using UnityEngine;
public class ChainLightning_Logic : MonoBehaviour
{
    public float speed = 5f;
    void Update() => transform.Translate(Vector3.up * speed * Time.deltaTime);
    void Start() => Destroy(gameObject, 5f); // ”ничтожаетс€ через 5 сек
}