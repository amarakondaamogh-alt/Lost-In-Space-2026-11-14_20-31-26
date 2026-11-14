using UnityEngine;

public class DoorController : MonoBehaviour
{
    private bool unlocked;

    private void Start()
    {
        GameManager.Instance.OnProgressChanged += CheckDoor;
        CheckDoor();
    }

    private void CheckDoor()
    {
        if (GameManager.Instance.doorUnlocked && !unlocked)
        {
            unlocked = true;
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnProgressChanged -= CheckDoor;
        }
    }
}