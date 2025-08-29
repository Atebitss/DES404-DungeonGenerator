using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashscreenManager : MonoBehaviour
{
    public void LoadMainMenu()
    {
        //load the main menu scene
        SceneManager.LoadScene("2.1 MainMenu");
    }
}
