using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    public PhysicsMaterial2D bouncyMaterial;
    public string sceneAfterDeath = "MainMenu";
    public float returnToMenuDelay = 2f;
    private bool isDead = false;

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        VisualSfxPlayer.PlayPlayerDeath();

        // 1. ˜˜˜˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜˜
        // ˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜, ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜ ˜˜˜˜ ˜ ˜˜˜˜˜˜˜˜
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // ˜˜˜˜˜˜˜˜˜ ˜˜, ˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜
            if (script is PlayerController2D || script is PlayerAttack2D || script is WeaponAim)
            {
                script.enabled = false;
            }
        }

        // 2. ˜˜˜˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ Pivot ˜ ˜˜˜˜˜˜˜, ˜˜˜˜˜ ˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜
        Transform weapon = transform.Find("WeaponPivot");
        if (weapon != null)
        {
            weapon.gameObject.SetActive(false); // ˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜
        }

        // 3. ˜˜˜˜˜˜ ˜˜˜˜˜
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1.0f;

        // ˜˜˜˜˜: ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜, ˜˜˜˜˜ ˜˜˜˜ ˜˜˜ ˜˜˜˜˜˜˜˜˜˜˜
        rb.constraints = RigidbodyConstraints2D.None;

        // ˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜
        GetComponent<Collider2D>().sharedMaterial = bouncyMaterial;

        // ˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜
        rb.AddForce(new Vector2(Random.Range(-2f, 2f), 5f), ForceMode2D.Impulse);
        rb.angularVelocity = Random.Range(-200f, 200f);

        Debug.Log("˜˜˜˜˜ ˜˜˜˜˜, ˜˜˜˜˜˜˜˜˜˜ ˜˜˜˜˜˜˜˜˜.");
        StartCoroutine(LoadSceneAfterDelay());
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(returnToMenuDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneAfterDeath, LoadSceneMode.Single);
    }
}