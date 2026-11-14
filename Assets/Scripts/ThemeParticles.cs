using UnityEngine;

// Lots of little sprites (snow, embers, bubbles, rain, dust...) that drift around the camera.
// When one leaves the screen it wraps back in on the other side, so there are always some visible.
// Created by LevelThemeManager - you don't need to add this by hand.
[DefaultExecutionOrder(1000)] // after the camera moves, so nothing jitters
public class ThemeParticles : MonoBehaviour
{
    public Sprite sprite;
    public Material material;
    public int count = 60;
    public Color color = Color.white;
    public Vector2 minVelocity = new Vector2(-0.3f, -1.5f);
    public Vector2 maxVelocity = new Vector2(0.3f, -0.5f);
    public Vector2 minSize = new Vector2(0.08f, 0.08f);
    public Vector2 maxSize = new Vector2(0.16f, 0.16f);
    public float wobble = 0.3f;      // side-to-side sway
    public float twinkle = 0f;       // 0 = steady, 1 = strong flicker
    public int sortingOrder = -5;

    private Transform[] bits;
    private SpriteRenderer[] renderers;
    private Vector2[] velocity;
    private float[] phase;
    private float[] baseAlpha;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        bits = new Transform[count];
        renderers = new SpriteRenderer[count];
        velocity = new Vector2[count];
        phase = new float[count];
        baseAlpha = new float[count];

        Vector2 center = cam != null ? (Vector2)cam.transform.position : Vector2.zero;
        Vector2 half = HalfView();

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("bit");
            go.transform.SetParent(transform, false);
            float s = Random.value;
            go.transform.localScale = new Vector3(Mathf.Lerp(minSize.x, maxSize.x, s), Mathf.Lerp(minSize.y, maxSize.y, s), 1f);
            go.transform.position = center + new Vector2(Random.Range(-half.x, half.x), Random.Range(-half.y, half.y));

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            if (material != null) sr.sharedMaterial = material;
            sr.sortingOrder = sortingOrder;
            baseAlpha[i] = color.a * Random.Range(0.6f, 1f);
            sr.color = new Color(color.r, color.g, color.b, baseAlpha[i]);

            bits[i] = go.transform;
            renderers[i] = sr;
            velocity[i] = new Vector2(Random.Range(minVelocity.x, maxVelocity.x), Random.Range(minVelocity.y, maxVelocity.y));
            phase[i] = Random.value * 10f;
        }
    }

    private Vector2 HalfView()
    {
        if (cam == null) return new Vector2(12f, 7f);
        float h = cam.orthographicSize + 1f;
        return new Vector2(h * cam.aspect + 1f, h);
    }

    private void LateUpdate()
    {
        if (bits == null || cam == null) return;

        Vector2 c = cam.transform.position;
        Vector2 half = HalfView();
        float t = Time.time;
        float dt = Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = bits[i].position;
            p.x += (velocity[i].x + Mathf.Sin(t * 2f + phase[i]) * wobble) * dt;
            p.y += velocity[i].y * dt;

            // Wrap around the edges of the screen.
            if (p.x < c.x - half.x) p.x += half.x * 2f;
            else if (p.x > c.x + half.x) p.x -= half.x * 2f;
            if (p.y < c.y - half.y) p.y += half.y * 2f;
            else if (p.y > c.y + half.y) p.y -= half.y * 2f;

            bits[i].position = p;

            if (twinkle > 0f)
            {
                float a = baseAlpha[i] * (1f - twinkle * 0.5f * (1f + Mathf.Sin(t * 3f + phase[i] * 7f)));
                renderers[i].color = new Color(color.r, color.g, color.b, a);
            }
        }
    }
}
