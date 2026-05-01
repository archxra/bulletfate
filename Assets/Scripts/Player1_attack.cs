using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack2D : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint; // ˜˜˜˜˜˜˜˜ ˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜
    public float attackCooldown = 0.8f;

    [Tooltip("˜˜˜˜˜˜˜˜ ˜˜˜˜˜ (˜˜˜˜˜˜ WeaponSprite)")]
    public Animator weaponAnimator;

    private float nextFireTime = 0f;
    private DiegoSfxPlayer diegoSfx;

    void Awake()
    {
        diegoSfx = GetComponent<DiegoSfxPlayer>();
        if (diegoSfx == null)
        {
            diegoSfx = gameObject.AddComponent<DiegoSfxPlayer>();
        }
    }

    void Update()
    {
        if (Mouse.current == null || Camera.main == null) return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Debug.Log("[DiegoSfx] LMB detected");
            Shoot();
            nextFireTime = Time.time + attackCooldown;
        }
    }

    void Shoot()
    {
        if (firePoint == null || bulletPrefab == null) return;

        // ˜ 2D ˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜˜ (firePoint ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜ ˜ ˜˜˜)
        // ˜˜˜˜ ˜˜˜˜ ˜˜˜˜˜ "˜˜˜˜˜", ˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜ (˜˜˜˜˜˜˜˜, +90 ˜˜˜ -90)
        // ˜ ˜˜˜˜˜ ˜˜˜˜˜˜, ˜˜˜˜ ˜˜˜˜˜˜ ˜˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜, ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜ ˜˜˜:

        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 10f));
        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)firePoint.position;

        // ˜˜˜˜˜˜˜ ˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜ 90 ˜˜˜˜˜˜˜˜ (˜˜˜˜˜˜˜˜ ˜˜˜ 2D-˜˜˜˜˜˜˜˜, ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
        diegoSfx?.PlayShoot();

        // ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Fire");
        }
    }
}