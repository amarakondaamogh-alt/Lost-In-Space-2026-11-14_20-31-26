using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (activated) return;

        activated = true;

        RespawnManager.Instance.SetCheckpoint(transform.position);

        GetComponent<SpriteRenderer>().color = Color.green;
    }
}