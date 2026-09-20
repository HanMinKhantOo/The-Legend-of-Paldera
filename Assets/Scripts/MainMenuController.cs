
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void NewGame()
    {
        // Start the opening story before gameplay.
        SceneManager.LoadScene("IntroStory");
    }

    public void ContinueGame()
    {
        // Keep the existing Continue behavior.
        SceneManager.LoadScene("Scene");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();
    }
}