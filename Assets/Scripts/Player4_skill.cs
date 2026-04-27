using UnityEngine;
using UnityEngine.InputSystem;

public class Mage_CrowSkill : MonoBehaviour
{
    public GameObject crowPrefab; // Префаб ворона
    public Transform firePoint;
    public float cooldown = 20f;
    private float nextFire;

    void Update()
    {
        // ПКМ - призыв ворона
        if (Mouse.current.rightButton.wasPressedThisFrame && Time.time >= nextFire)
        {
            Instantiate(crowPrefab, firePoint.position, transform.rotation);
            nextFire = Time.time + cooldown;
        }
    }
}