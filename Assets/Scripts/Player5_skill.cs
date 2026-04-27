using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Soldier_IncendiarySkill : MonoBehaviour
{
    public GameObject normalBullet; // Префаб с Bullet2D
    public GameObject fireBullet;   // Префаб с FireBullet_Logic
    public float duration = 15f;
    public float cooldown = 50f;

    private Soldier_Thompson weapon; // Скрипт атаки солдата
    private float nextUse;

    void Start() => weapon = GetComponent<Soldier_Thompson>();

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame && Time.time >= nextUse)
        {
            StartCoroutine(ActivateFireRounds());
            nextUse = Time.time + cooldown;
        }
    }

    IEnumerator ActivateFireRounds()
    {
        weapon.bulletPrefab = fireBullet;
        GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0f);
        yield return new WaitForSeconds(duration);
        weapon.bulletPrefab = normalBullet;
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}