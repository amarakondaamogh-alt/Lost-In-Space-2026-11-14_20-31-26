using UnityEngine;

public class PressureSwitch : MonoBehaviour
{
    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player"))
            return;

        activated = true;

        GameManager.Instance.ActivateSwitch();

        GetComponent<SpriteRenderer>().color = Color.green;
    }
}