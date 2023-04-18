using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    // Start is called before the first frame update
    public GameObject c1, c2;

    public void BackButton() {
        c1.SetActive(true);
        c2.SetActive(false);
    }

    public void HelpScreen() {
        c1.SetActive(false);
        c2.SetActive(true);
    }
    public void StartGameSinglePlayer() {
        Pref.I.twoPlayers = false;
        SceneManager.LoadScene(1);
    }
    public void StartGameTwoPlayer() {
        Pref.I.twoPlayers = true;
        Pref.I.score1 = 0;
        Pref.I.score2 = 0;
        SceneManager.LoadScene(1);
    }
    public void QuitGame() {
        Application.Quit();
    }
}
