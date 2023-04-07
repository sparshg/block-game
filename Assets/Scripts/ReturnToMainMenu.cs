using UnityEngine;
using UnityEngine.SceneManagement;
public class ReturnToMainMenu : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
