using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int crystalsCollected = 0;
    public int crystalsRequired = 3;

    public bool switchActivated = false;
    public bool doorUnlocked = false;
    public bool gameWon = false;

    // Seconds spent in this level (stops while paused or once the level is won).
    public float levelTime = 0f;

    [Tooltip("Optional: drag the WinScreen object here so it shows when you win.")]
    public WinScreen winScreen;

    // Public so DoorController and GameUI can subscribe to it.
    public event Action OnProgressChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!gameWon)
            levelTime += Time.deltaTime;
    }

    public void CollectCrystal()
    {
        crystalsCollected++;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayCrystal();

        CheckProgress();
    }

    public void ActivateSwitch()
    {
        switchActivated = true;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySwitch();

        CheckProgress();
    }

    public void CheckProgress()
    {
        if (crystalsCollected >= crystalsRequired && switchActivated)
        {
            doorUnlocked = true;
        }

        OnProgressChanged?.Invoke();
    }

    public void WinGame()
    {
        if (gameWon) return;

        gameWon = true;
        Debug.Log("MISSION COMPLETE!");

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayWin();

        string stats = SaveAndDescribeTime();

        if (winScreen != null)
            winScreen.ShowWinScreen(stats);
        else
            Time.timeScale = 0f;
    }

    // Saves the best time for this level and returns text like "Time: 12.3s  Best: 10.1s".
    private string SaveAndDescribeTime()
    {
        string key = "best_" + SceneManager.GetActiveScene().name;
        float best = PlayerPrefs.GetFloat(key, -1f);
        bool isNewBest = best < 0f || levelTime < best;

        if (isNewBest)
        {
            best = levelTime;
            PlayerPrefs.SetFloat(key, best);
            PlayerPrefs.Save();
        }

        return "Time: " + levelTime.ToString("0.0") + "s     Best: " + best.ToString("0.0") + "s" +
               (isNewBest ? "     NEW BEST!" : "");
    }
}
