using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("Scene");
    }

    public void ContinueGame()
    {
        // Temporary: same behavior as New Game.
        SceneManager.LoadScene("Scene");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();
    }
}