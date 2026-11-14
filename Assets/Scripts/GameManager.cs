using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int crystalsCollected = 0;
    public int crystalsRequired = 3;

    public bool switchActivated = false;
    public bool doorUnlocked = false;
    public bool gameWon = false;

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

        if (winScreen != null)
            winScreen.ShowWinScreen();
        else
            Time.timeScale = 0f;
    }
}
