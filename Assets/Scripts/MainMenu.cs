using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Название сцены с игрой
    public string gameSceneName = "Level_1";

    // Нажали Play
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Нажали Exit
    public void ExitGame()
    {
        Debug.Log("Game closed");

        Application.Quit();
    }
}