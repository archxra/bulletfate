using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    public SpriteRenderer weaponRenderer; // Сюда закинь объект WeaponSprite

    void Update()
    {
        if (Mouse.current == null || Camera.main == null) return;

        Vector2 screenMousePos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 10f));

        Vector2 lookDir = mouseWorldPos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // Крутим Pivot
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Переворачиваем сам спрайт ружья
        if (weaponRenderer != null)
        {
            weaponRenderer.flipY = (angle > 90 || angle < -90);
        }
    }
}