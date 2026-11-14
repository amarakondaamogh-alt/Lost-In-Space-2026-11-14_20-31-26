using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;

    [Header("Vertical follow")]
    public bool followY = true;
    public float minY = 0f;   // camera never goes lower than this (keeps the ground in view)

    void LateUpdate()
    {
        if (player == null) return;

        float targetY = followY ? Mathf.Max(minY, player.position.y) : transform.position.y;

        Vector3 target = new Vector3(player.position.x, targetY, -10f);

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            smoothSpeed * Time.deltaTime
        );
    }
}
