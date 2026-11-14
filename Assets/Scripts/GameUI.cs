using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public TMP_Text crystalText;
    public TMP_Text objectiveText;
    public TMP_Text timeText;

    private void Start()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnProgressChanged += UpdateUI;
        UpdateUI();
    }

    private void Update()
    {
        if (timeText != null && GameManager.Instance != null)
            timeText.text = "Time: " + GameManager.Instance.levelTime.ToString("0.0") + "s";
    }

    private void UpdateUI()
    {
        GameManager gm = GameManager.Instance;

        if (crystalText != null)
        {
            crystalText.text = "Crystals: " + gm.crystalsCollected + "/" + gm.crystalsRequired;
        }

        if (objectiveText == null) return;

        if (gm.doorUnlocked)
        {
            objectiveText.text = "Objective: Go through the door and return to your spaceship!";
        }
        else if (gm.switchActivated)
        {
            objectiveText.text = "Objective: Collect the remaining crystals!";
        }
        else
        {
            objectiveText.text = "Objective: Find the crystals and activate the switch!";
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnProgressChanged -= UpdateUI;
    }
}
