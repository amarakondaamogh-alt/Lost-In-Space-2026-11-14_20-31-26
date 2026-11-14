using UnityEngine;

// Put this on a wide trigger collider below the level.
// If the player falls off, they respawn at the last checkpoint.
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayDamage();

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.Respawn();
    }
}
