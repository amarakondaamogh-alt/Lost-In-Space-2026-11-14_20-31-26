using UnityEngine;

// Sends the player back to the last checkpoint on touch. If pointA/pointB are set it
// patrols between them; if not, it just stays put as a static hazard.
public class MovingHazard : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 3f;

    private Vector3 target;

    private void Start()
    {
        if (pointB != null)
            target = pointB.position;
    }

    private void Update()
    {
        if (pointA == null || pointB == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            target = target == pointA.position
                ? pointB.position
                : pointA.position;
        }
    }

    // Works whether the hazard's collider is a trigger or solid.
    private void OnTriggerEnter2D(Collider2D other)
    {
        HurtPlayer(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HurtPlayer(collision.gameObject);
    }

    private void HurtPlayer(GameObject obj)
    {
        if (!obj.CompareTag("Player")) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayDamage();

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.Respawn();
    }
}
