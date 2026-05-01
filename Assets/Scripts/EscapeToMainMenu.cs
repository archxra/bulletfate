using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EscapeToMainMenu : MonoBehaviour
{
    private static EscapeToMainMenu instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        GameObject go = new GameObject("EscapeToMainMenu");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<EscapeToMainMenu>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;
        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
