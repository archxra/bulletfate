using UnityEngine;
using UnityEngine.InputSystem;

public class Skill_FernandoUltra : MonoBehaviour
{
    public GameObject ultraPrefab; // ˜˜˜˜˜˜ Fernando_Ultra_Visual
    public float cooldown = 90f;
    private float nextUse;
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
        // ˜˜˜˜˜ ˜˜ ˜˜˜˜˜˜ R
        if (Keyboard.current.rKey.wasPressedThisFrame && Time.time >= nextUse)
        {
            Debug.Log("[DiegoSfx] R detected");
            Activate();
            StartCoroutine(PlayUltimateSfxWithDelay());
            nextUse = Time.time + cooldown;
        }
    }

    void Activate()
    {
        // ˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜
        // ˜˜˜ ˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜, ˜˜˜˜˜˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜
        Instantiate(ultraPrefab, transform.position, Quaternion.identity);
    }

    private System.Collections.IEnumerator PlayUltimateSfxWithDelay()
    {
        yield return new WaitForSeconds(1f);
        diegoSfx?.PlayUltimate();
    }
}