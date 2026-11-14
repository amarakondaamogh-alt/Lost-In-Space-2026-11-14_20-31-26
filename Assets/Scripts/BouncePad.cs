using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public float bounceForce = 18f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayBounce();
        }
    }
}
