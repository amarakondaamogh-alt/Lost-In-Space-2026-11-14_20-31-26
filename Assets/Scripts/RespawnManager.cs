using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    private Vector3 checkpointPosition;
    private GameObject player;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            checkpointPosition = player.transform.position;
        else
            Debug.LogWarning("RespawnManager: no object tagged 'Player' found in this scene.");
    }

    private void Update()
    {
        // Press R any time to go back to the last checkpoint (handy if you get stuck).
        if (Time.timeScale > 0f && GameInput.RespawnPressed())
            Respawn();
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }

    public void Respawn()
    {
        if (player == null) return;

        player.transform.SetParent(null);
        player.transform.position = checkpointPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
