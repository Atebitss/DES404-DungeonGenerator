using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnStartPressed()
    {
        Debug.Log("Start button pressed. Load the game scene.");
        SceneManager.LoadScene("3.1 DungeonRealm");
    }

    public void OnOptionsPressed()
    {
        Debug.Log("Options button pressed. Open options menu.");
        // Add code to open the options menu here
    }

    public void OnExitPressed()
    {
        Debug.Log("Exit button pressed. Quit the application.");
        Application.Quit();
    }
}
