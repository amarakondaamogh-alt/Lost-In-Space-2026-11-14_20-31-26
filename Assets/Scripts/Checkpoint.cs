using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("How far above this object the player is put back when respawning here.")]
    public float respawnLift = 1f;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (activated) return;

        activated = true;

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.SetCheckpoint(transform.position + Vector3.up * respawnLift);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.green;
    }
}
