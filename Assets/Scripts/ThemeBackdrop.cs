using UnityEngine;

// Big far-away things (planets, suns, clouds) behind the level. They follow the camera most of the
// way, so they move slowly across the screen and feel far away (parallax).
// Created by LevelThemeManager - you don't need to add this by hand.
[DefaultExecutionOrder(1000)]
public class ThemeBackdrop : MonoBehaviour
{
    [Range(0f, 1f)] public float follow = 0.9f;   // 1 = glued to the camera, 0 = part of the level
    private Camera cam;
    private Vector3 start;

    private void Start()
    {
        cam = Camera.main;
        start = cam != null ? cam.transform.position : Vector3.zero;
    }

    private void LateUpdate()
    {
        if (cam == null) return;
        Vector3 c = cam.transform.position;
        transform.position = new Vector3(start.x + (c.x - start.x) * follow, start.y + (c.y - start.y) * follow, 0f);
    }
}
