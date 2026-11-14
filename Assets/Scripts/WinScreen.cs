using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Shown when the player reaches the ship. NextLevel loads the next scene in Build Settings
// (Level 10 leads on to the WinScene, which leads to the Credits).
public class WinScreen : MonoBehaviour
{
    public GameObject winPanel;
    public TMP_Text statsText;

    public void ShowWinScreen(string stats = "")
    {
        if (statsText != null) statsText.text = stats;
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings)
            next = 0; // back to the main menu

        SceneManager.LoadScene(next);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
