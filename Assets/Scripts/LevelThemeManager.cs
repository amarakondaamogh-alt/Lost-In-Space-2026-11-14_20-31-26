using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Gives every level its own world. Runs by itself whenever a level scene loads -
// nothing to drag in. Edit the Themes list below to change colours or physics.
//
//  1 Lost in Space  - normal                    6 Crystal Caves  - sparkly purple cave
//  2 Ice Planet     - slippery ground, snow     7 Toxic Swamp    - toxic floor, green bubbles
//  3 Lava Planet    - lava floor, embers        8 Storm Planet   - rain and lightning
//  4 Ocean Planet   - floaty underwater jumps   9 The Moon       - low gravity, big jumps
//  5 Desert Planet  - blowing sand, twin suns  10 Earth          - blue sky, grass, clouds
public class LevelThemeManager : MonoBehaviour
{
    public enum Weather { Stars, Snow, Embers, Bubbles, Dust, Sparkles, ToxicBubbles, Rain, Clouds }

    public class Theme
    {
        public string title;
        public string hint;
        public Color sky;
        public Color ground;
        public Color platform;
        public bool tint = true;            // false = keep the level's original colours
        public Weather weather;
        public Color weatherColor = Color.white;
        public bool deadlyFloor;            // lava / toxic floor (stay on the platforms!)
        public Color floorColor;
        public bool lightning;
        // Player physics (0 = leave unchanged)
        public float gravityScale, jumpForce, moveSpeed, acceleration;
        // Background objects: colour, size, position relative to the starting camera
        public (Color color, float size, Vector2 pos)[] backdrop = new (Color, float, Vector2)[0];
    }

    static readonly string[] LevelScenes =
    {
        "MainLevel", "Level02", "Level03", "Level04", "Level05",
        "Level06", "Level07", "Level08", "Level09", "Level10"
    };

    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    static readonly Theme[] Themes =
    {
        new Theme { title = "Lost in Space", hint = "Find the crystals and get back to your ship",
            sky = C(0.05f, 0.06f, 0.16f), tint = false, weather = Weather.Stars,
            backdrop = new[] { (C(0.55f, 0.35f, 0.75f, 0.8f), 5f, new Vector2(9f, 4f)), (C(0.8f, 0.8f, 0.9f, 0.7f), 1.2f, new Vector2(4f, 5.5f)) } },

        new Theme { title = "Ice Planet", hint = "The ground is slippery!",
            sky = C(0.12f, 0.25f, 0.42f), ground = C(0.85f, 0.95f, 1f), platform = C(0.6f, 0.85f, 1f),
            weather = Weather.Snow, acceleration = 10f,
            backdrop = new[] { (C(0.8f, 0.92f, 1f, 0.6f), 6f, new Vector2(-8f, 4f)) } },

        new Theme { title = "Lava Planet", hint = "Don't touch the lava floor!",
            sky = C(0.22f, 0.04f, 0.03f), ground = C(0.25f, 0.12f, 0.1f), platform = C(0.45f, 0.22f, 0.16f),
            weather = Weather.Embers, weatherColor = C(1f, 0.55f, 0.1f),
            deadlyFloor = true, floorColor = C(1f, 0.35f, 0.05f),
            backdrop = new[] { (C(1f, 0.3f, 0.1f, 0.55f), 9f, new Vector2(7f, 4f)) } },

        new Theme { title = "Ocean Planet", hint = "Underwater - your jumps are floaty",
            sky = C(0.02f, 0.2f, 0.32f), ground = C(0.78f, 0.7f, 0.45f), platform = C(0.95f, 0.5f, 0.45f),
            weather = Weather.Bubbles, weatherColor = C(0.75f, 0.92f, 1f, 0.55f),
            gravityScale = 1.5f, jumpForce = 11.3f, moveSpeed = 4.8f,
            backdrop = new[] { (C(0.5f, 0.85f, 0.95f, 0.25f), 12f, new Vector2(0f, 9f)) } },

        new Theme { title = "Desert Planet", hint = "Hot sand and twin suns",
            sky = C(0.35f, 0.2f, 0.12f), ground = C(0.88f, 0.68f, 0.38f), platform = C(0.7f, 0.5f, 0.28f),
            weather = Weather.Dust, weatherColor = C(0.95f, 0.8f, 0.55f, 0.5f),
            backdrop = new[] { (C(1f, 0.85f, 0.3f, 0.9f), 2.5f, new Vector2(5f, 4.5f)), (C(1f, 0.55f, 0.2f, 0.9f), 1.6f, new Vector2(8f, 3.2f)) } },

        new Theme { title = "Crystal Caves", hint = "Deep inside a glowing cave",
            sky = C(0.1f, 0.03f, 0.18f), ground = C(0.3f, 0.15f, 0.4f), platform = C(0.65f, 0.38f, 0.95f),
            weather = Weather.Sparkles, weatherColor = C(0.7f, 0.95f, 1f),
            backdrop = new[] { (C(0.6f, 0.3f, 0.9f, 0.25f), 8f, new Vector2(-6f, 3f)), (C(0.3f, 0.8f, 1f, 0.2f), 6f, new Vector2(8f, 5f)) } },

        new Theme { title = "Toxic Swamp", hint = "Toxic floor - stay on the platforms!",
            sky = C(0.06f, 0.14f, 0.05f), ground = C(0.2f, 0.3f, 0.12f), platform = C(0.42f, 0.56f, 0.2f),
            weather = Weather.ToxicBubbles, weatherColor = C(0.5f, 1f, 0.3f, 0.6f),
            deadlyFloor = true, floorColor = C(0.4f, 1f, 0.2f),
            backdrop = new[] { (C(0.55f, 0.75f, 0.2f, 0.5f), 7f, new Vector2(8f, 4f)) } },

        new Theme { title = "Storm Planet", hint = "Rain, wind and lightning",
            sky = C(0.1f, 0.11f, 0.15f), ground = C(0.3f, 0.32f, 0.36f), platform = C(0.48f, 0.5f, 0.58f),
            weather = Weather.Rain, weatherColor = C(0.7f, 0.8f, 1f, 0.6f), lightning = true,
            backdrop = new[] { (C(0.2f, 0.22f, 0.28f, 0.9f), 7f, new Vector2(-6f, 6f)), (C(0.18f, 0.2f, 0.25f, 0.9f), 8f, new Vector2(5f, 6.5f)) } },

        new Theme { title = "The Moon", hint = "Low gravity - big jumps! Earth is close...",
            sky = C(0.01f, 0.01f, 0.03f), ground = C(0.6f, 0.6f, 0.62f), platform = C(0.76f, 0.76f, 0.8f),
            weather = Weather.Stars, gravityScale = 1.6f, jumpForce = 13f,
            backdrop = new[] { (C(0.2f, 0.45f, 0.95f, 1f), 4f, new Vector2(8f, 4f)), (C(0.3f, 0.7f, 0.3f, 0.9f), 1.8f, new Vector2(7.4f, 4.5f)) } },

        new Theme { title = "Earth", hint = "Almost home!",
            sky = C(0.32f, 0.58f, 0.88f), ground = C(0.3f, 0.65f, 0.25f), platform = C(0.55f, 0.38f, 0.22f),
            weather = Weather.Clouds,
            backdrop = new[] { (C(1f, 0.95f, 0.5f, 1f), 2.5f, new Vector2(8f, 5f)) } },
    };

    // ------------------------------------------------------------------ start-up

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int index = System.Array.IndexOf(LevelScenes, scene.name);
        if (index < 0) return; // menus etc.

        var go = new GameObject("Level Theme");
        go.AddComponent<LevelThemeManager>().Apply(Themes[index], index + 1);
    }

    // ------------------------------------------------------------------ applying a theme

    private Theme theme;
    private Camera cam;
    private SpriteRenderer floor;
    private float nextLightning;
    private static Sprite circle;

    private void Apply(Theme t, int levelNumber)
    {
        theme = t;
        cam = Camera.main;

        GameObject ground = GameObject.Find("Ground");
        SpriteRenderer groundSr = ground != null ? ground.GetComponent<SpriteRenderer>() : null;
        Sprite square = groundSr != null ? groundSr.sprite : null;
        Material mat = groundSr != null ? groundSr.sharedMaterial : null;

        // Sky
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = t.sky;
        }

        // Ground and platform colours
        if (t.tint)
        {
            foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
            {
                string n = sr.gameObject.name;
                if (n.StartsWith("Ground")) sr.color = t.ground;
                else if (n.StartsWith("Platform") && !n.StartsWith("Platform Hazard")) sr.color = t.platform;
            }
        }

        // Player physics
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null)
        {
            if (t.moveSpeed > 0f) pc.moveSpeed = t.moveSpeed;
            if (t.jumpForce > 0f) pc.jumpForce = t.jumpForce;
            pc.acceleration = t.acceleration;
            var rb = pc.GetComponent<Rigidbody2D>();
            if (rb != null && t.gravityScale > 0f) rb.gravityScale = t.gravityScale;
        }

        if (t.deadlyFloor && ground != null && square != null) MakeDeadlyFloor(ground, square, mat);
        LevelSpikes.Place(levelNumber, ground, mat, floor != null ? floor.bounds.max.x : float.NaN);
        MakeBackdrop(mat);
        MakeWeather(square, mat);

        nextLightning = Time.time + Random.Range(3f, 6f);
        StartCoroutine(ShowBanner(levelNumber));
    }

    // A glowing strip on top of the ground that sends you back to the checkpoint.
    // It stops at the end of the last platform so the door and ship can still be reached.
    private void MakeDeadlyFloor(GameObject ground, Sprite square, Material mat)
    {
        Vector3 gp = ground.transform.position;
        Vector3 gs = ground.transform.lossyScale;
        float groundTop = gp.y + gs.y * 0.5f;
        float left = gp.x - gs.x * 0.5f;

        GameObject door = GameObject.Find("Door");
        GameObject ship = GameObject.Find("Spaceship");
        float limit = door != null ? door.transform.position.x : ship != null ? ship.transform.position.x : float.MaxValue;

        float right = float.MinValue;
        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude))
        {
            if (!sr.gameObject.name.StartsWith("Platform") || sr.gameObject.name.StartsWith("Platform Hazard")) continue;
            Bounds b = sr.bounds;
            if (b.center.x < limit) right = Mathf.Max(right, b.max.x);
        }
        if (right <= left) return;
        right -= 0.2f;

        var go = new GameObject("Deadly Floor");
        go.transform.position = new Vector3((left + right) * 0.5f, groundTop + 0.1f, 0f);
        go.transform.localScale = new Vector3(right - left, 0.4f, 1f);
        floor = go.AddComponent<SpriteRenderer>();
        floor.sprite = square;
        if (mat != null) floor.sharedMaterial = mat;
        floor.color = theme.floorColor;
        floor.sortingOrder = 1;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        go.AddComponent<MovingHazard>(); // no patrol points = a still hazard that resets the player
    }

    private void MakeBackdrop(Material mat)
    {
        if (theme.backdrop.Length == 0) return;
        var root = new GameObject("Theme Backdrop");
        root.AddComponent<ThemeBackdrop>().follow = 0.9f;
        Vector3 origin = cam != null ? cam.transform.position : Vector3.zero;
        root.transform.position = new Vector3(origin.x, origin.y, 0f);

        int order = -20;
        foreach (var (color, size, pos) in theme.backdrop)
        {
            var go = new GameObject("Backdrop");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Circle();
            if (mat != null) sr.sharedMaterial = mat;
            sr.color = color;
            sr.sortingOrder = order++;
        }
    }

    private void MakeWeather(Sprite square, Material mat)
    {
        var go = new GameObject("Theme Weather");
        var p = go.AddComponent<ThemeParticles>();
        p.material = mat;
        p.color = theme.weatherColor;
        p.sprite = Circle();

        switch (theme.weather)
        {
            case Weather.Stars:
                p.count = 90; p.minVelocity = Vector2.zero; p.maxVelocity = Vector2.zero; p.wobble = 0f;
                p.minSize = Vector2.one * 0.05f; p.maxSize = Vector2.one * 0.14f; p.twinkle = 0.8f; p.sortingOrder = -30;
                break;
            case Weather.Snow:
                p.count = 120; p.minVelocity = new Vector2(-0.4f, -1.8f); p.maxVelocity = new Vector2(0.4f, -0.7f);
                p.minSize = Vector2.one * 0.07f; p.maxSize = Vector2.one * 0.18f; p.wobble = 0.5f; p.sortingOrder = 10;
                break;
            case Weather.Embers:
                p.count = 70; p.minVelocity = new Vector2(-0.3f, 0.8f); p.maxVelocity = new Vector2(0.3f, 2.4f);
                p.minSize = Vector2.one * 0.06f; p.maxSize = Vector2.one * 0.14f; p.wobble = 0.6f; p.twinkle = 0.6f;
                break;
            case Weather.Bubbles:
            case Weather.ToxicBubbles:
                p.count = 50; p.minVelocity = new Vector2(0f, 0.6f); p.maxVelocity = new Vector2(0f, 1.6f);
                p.minSize = Vector2.one * 0.1f; p.maxSize = Vector2.one * 0.35f; p.wobble = 0.5f;
                break;
            case Weather.Dust:
                p.count = 80; p.minVelocity = new Vector2(3f, -0.3f); p.maxVelocity = new Vector2(7f, 0.3f);
                p.minSize = Vector2.one * 0.05f; p.maxSize = Vector2.one * 0.12f; p.wobble = 0.2f; p.sortingOrder = 10;
                break;
            case Weather.Sparkles:
                p.count = 70; p.minVelocity = new Vector2(-0.1f, -0.1f); p.maxVelocity = new Vector2(0.1f, 0.1f);
                p.minSize = Vector2.one * 0.06f; p.maxSize = Vector2.one * 0.16f; p.wobble = 0.1f; p.twinkle = 1f;
                break;
            case Weather.Rain:
                p.sprite = square != null ? square : Circle();
                p.count = 160; p.minVelocity = new Vector2(-3f, -16f); p.maxVelocity = new Vector2(-2f, -12f);
                p.minSize = new Vector2(0.03f, 0.35f); p.maxSize = new Vector2(0.04f, 0.55f); p.wobble = 0f; p.sortingOrder = 10;
                break;
            case Weather.Clouds:
                p.count = 9; p.minVelocity = new Vector2(0.3f, 0f); p.maxVelocity = new Vector2(0.8f, 0f);
                p.minSize = new Vector2(2.5f, 1f); p.maxSize = new Vector2(5f, 1.8f); p.wobble = 0f;
                p.color = C(1f, 1f, 1f, 0.75f); p.sortingOrder = -15;
                break;
        }
    }

    // ------------------------------------------------------------------ every frame

    private void Update()
    {
        if (theme == null) return;

        // Lava / toxic floor glows.
        if (floor != null)
        {
            float glow = 0.5f + 0.5f * Mathf.Sin(Time.time * 3f);
            floor.color = Color.Lerp(theme.floorColor, Color.Lerp(theme.floorColor, Color.white, 0.4f), glow);
        }

        // Lightning flashes.
        if (theme.lightning && cam != null && Time.time >= nextLightning && Time.timeScale > 0f)
        {
            nextLightning = Time.time + Random.Range(4f, 9f);
            StartCoroutine(Flash());
        }
    }

    private IEnumerator Flash()
    {
        Color bright = Color.Lerp(theme.sky, Color.white, 0.65f);
        for (int i = 0; i < 2; i++)
        {
            cam.backgroundColor = bright;
            yield return new WaitForSeconds(0.06f);
            cam.backgroundColor = theme.sky;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // "LEVEL 2 - ICE PLANET" across the screen for a few seconds, and the theme name in the HUD.
    private IEnumerator ShowBanner(int levelNumber)
    {
        GameObject hud = GameObject.Find("HUD Canvas");
        if (hud == null) yield break;

        Transform levelText = hud.transform.Find("Level Text");
        if (levelText != null && levelText.TryGetComponent(out TMP_Text lt))
            lt.text = "Level " + levelNumber + " / " + LevelScenes.Length + "  -  " + theme.title;

        var go = new GameObject("Theme Banner", typeof(RectTransform));
        go.transform.SetParent(hud.transform, false);
        go.transform.SetAsFirstSibling(); // stays behind the pause / win panels
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 260f);
        rt.sizeDelta = new Vector2(1600f, 220f);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = "<size=100>" + theme.title.ToUpper() + "</size>\n<size=44>" + theme.hint + "</size>";
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.color = Color.white;

        yield return new WaitForSecondsRealtime(2.5f);
        for (float a = 1f; a > 0f; a -= Time.unscaledDeltaTime)
        {
            text.alpha = a;
            yield return null;
        }
        Destroy(go);
    }

    // A soft round sprite made in code (used for planets, bubbles, snow...).
    private static Sprite Circle()
    {
        if (circle != null) return circle;

        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color32[size * size];
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                byte a = (byte)(255 * Mathf.Clamp01((1f - d) * 6f)); // solid with a soft edge
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        tex.SetPixels32(pixels);
        tex.Apply();
        circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circle;
    }
}
