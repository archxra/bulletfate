using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon_PilumThrower : MonoBehaviour
{
    public GameObject pilumPrefab;
    public Transform firePoint;
    public float cooldown = 0.7f; // кд из дока
    private float nextFire;

    void Update()
    {
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFire)
        {
            Instantiate(pilumPrefab, firePoint.position, transform.rotation);
            nextFire = Time.time + cooldown;
        }
    }
}