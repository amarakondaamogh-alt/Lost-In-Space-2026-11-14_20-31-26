using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 16f;
    [Tooltip("0 = stop and turn instantly. Small numbers = slippery, like ice.")]
    public float acceleration = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float lastGroundedTime = -10f;
    private float lastJumpTime = -10f;

    private const float GroundedGrace = 0.1f;   // small grace window so jumping feels reliable
    private const float JumpCooldown = 0.2f;    // stops accidental double jumps

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // paused or level finished

        float move = GameInput.Horizontal();
        float targetX = move * moveSpeed;
        float newX = acceleration > 0f
            ? Mathf.MoveTowards(rb.linearVelocity.x, targetX, acceleration * Time.deltaTime) // slippery (ice)
            : targetX;                                                                         // instant
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        if (spriteRenderer != null && Mathf.Abs(move) > 0.01f)
            spriteRenderer.flipX = move < 0f;

        bool grounded = Time.time - lastGroundedTime <= GroundedGrace;
        if (GameInput.JumpPressed() && grounded && Time.time - lastJumpTime > JumpCooldown)
        {
            Jump();
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        lastJumpTime = Time.time;
        lastGroundedTime = -10f;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayJump();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time - lastJumpTime < JumpCooldown) return;

        // Only count surfaces under the player (not walls or ceilings).
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                lastGroundedTime = Time.time;
                return;
            }
        }
    }
}
