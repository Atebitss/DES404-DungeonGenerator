using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialisationManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Starting SaffensDescent.exe, loading 1.1 SplashScreen scene.");
        SceneManager.LoadScene("1.1 SplashScreen");
    }
}
