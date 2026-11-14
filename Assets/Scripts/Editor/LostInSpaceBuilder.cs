#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

// Adds a menu item: Lost in Space > Build Game Scenes
// It creates a MainMenu scene and a playable MainLevel scene,
// wires up every script, and adds both scenes to Build Settings.
public static class LostInSpaceBuilder
{
    const string ArtFolder = "Assets/Art";
    const string SceneFolder = "Assets/Scenes";
    const string SpritePath = ArtFolder + "/Square.png";
    const string MaterialPath = ArtFolder + "/NoFriction.physicsMaterial2D";

    static Sprite square;

    static readonly Color GroundColor = new Color(0.35f, 0.32f, 0.5f);
    static readonly Color CrystalColor = new Color(0.3f, 0.95f, 1f);
    static readonly Color HazardColor = new Color(1f, 0.25f, 0.3f);
    static readonly Color PadColor = new Color(0.3f, 1f, 0.45f);
    static readonly Color SpaceColor = new Color(0.04f, 0.03f, 0.12f);

    [MenuItem("Lost in Space/Build Game Scenes")]
    public static void BuildAll()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            EditorUtility.DisplayDialog("Lost in Space",
                "TextMeshPro resources are missing.\n\nGo to Window > TextMeshPro > Import TMP Essential Resources, " +
                "click Import, then run Lost in Space > Build Game Scenes again.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Lost in Space",
            "This creates MainMenu and MainLevel scenes in Assets/Scenes. Scenes with those names will be replaced. Continue?",
            "Build", "Cancel"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Directory.CreateDirectory(ArtFolder);
        Directory.CreateDirectory(SceneFolder);
        AssetDatabase.Refresh();

        square = MakeSquareSprite();
        PhysicsMaterial2D noFriction = MakeNoFrictionMaterial();

        string levelPath = BuildLevel(noFriction);
        string menuPath = BuildMenu();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(menuPath, true),
            new EditorBuildSettingsScene(levelPath, true)
        };

        EditorSceneManager.OpenScene(levelPath);

        EditorUtility.DisplayDialog("Lost in Space",
            "Done! MainLevel is open. Press Play to test.\n\n" +
            "Controls: A/D or arrows to move, Space to jump, Esc to pause.\n" +
            "Goal: grab 3 crystals, step on the switch, then reach the ship.", "Let's go");
    }

    // ---------- Assets ----------

    static Sprite MakeSquareSprite()
    {
        if (!File.Exists(SpritePath))
        {
            var tex = new Texture2D(16, 16);
            var px = new Color[16 * 16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(SpritePath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(SpritePath);
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; // needed for tiled sprites
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }

    static PhysicsMaterial2D MakeNoFrictionMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, MaterialPath);
            AssetDatabase.SaveAssets();
        }
        return mat;
    }

    // ---------- Level scene ----------

    static string BuildLevel(PhysicsMaterial2D noFriction)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        Camera cam = MakeCamera();
        var follow = cam.gameObject.AddComponent<CameraFollow>();

        // Background stars (decoration only)
        var stars = new GameObject("Stars");
        Random.InitState(7);
        for (int i = 0; i < 150; i++)
        {
            float s = Random.Range(0.05f, 0.15f);
            var star = MakeSpriteObject("Star",
                new Vector2(Random.Range(-20f, 60f), Random.Range(-6f, 6f)),
                new Vector2(s, s),
                new Color(1f, 1f, 1f, Random.Range(0.3f, 0.9f)));
            star.GetComponent<SpriteRenderer>().sortingOrder = -10;
            star.transform.SetParent(stars.transform, true);
        }

        // Managers
        var managers = new GameObject("Managers");
        var gm = managers.AddComponent<GameManager>();
        gm.crystalsRequired = 3;
        managers.AddComponent<RespawnManager>();
        var sm = managers.AddComponent<SoundManager>();
        sm.audioSource = managers.AddComponent<AudioSource>();
        sm.audioSource.playOnAwake = false;

        // Player
        var player = Box("Player", new Vector2(-8f, -3f), new Vector2(0.8f, 1.2f), new Color(1f, 0.6f, 0.2f));
        player.tag = "Player";
        player.GetComponent<SpriteRenderer>().sortingOrder = 5;
        player.GetComponent<BoxCollider2D>().sharedMaterial = noFriction;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        player.AddComponent<PlayerController>();
        follow.player = player.transform;

        // Level geometry
        var level = new GameObject("Level");
        Parent(level, Box("Ground 1", new Vector2(0f, -4.5f), new Vector2(20f, 1f), GroundColor));
        Parent(level, Box("Ground 2", new Vector2(22f, -4.5f), new Vector2(16f, 1f), GroundColor));
        Parent(level, Box("Ground 3", new Vector2(41f, -4.5f), new Vector2(14f, 1f), GroundColor));
        Parent(level, Box("Left Wall", new Vector2(-10.5f, 0f), new Vector2(1f, 12f), GroundColor));
        Parent(level, Box("Right Wall", new Vector2(48.5f, 0f), new Vector2(1f, 12f), GroundColor));
        Parent(level, Box("High Ledge", new Vector2(8.5f, 1.5f), new Vector2(3f, 0.5f), GroundColor));

        // Crystals
        var crystals = new GameObject("Crystals");
        MakeCrystal(crystals, new Vector2(-3f, -3f));
        MakeCrystal(crystals, new Vector2(8.5f, 2.5f));   // on the high ledge
        MakeCrystal(crystals, new Vector2(27f, -1.5f));   // jump to reach

        // Bounce pad (launches you up to the high ledge)
        var pad = Box("Bounce Pad", new Vector2(5f, -3.85f), new Vector2(1.5f, 0.3f), PadColor, trigger: true);
        pad.AddComponent<BouncePad>().bounceForce = 22f;

        // Moving platforms over the gaps
        MakeMovingPlatform("Moving Platform 1", new Vector2(11f, -4.2f), new Vector2(13f, -4.2f));
        MakeMovingPlatform("Moving Platform 2", new Vector2(31f, -4.2f), new Vector2(33f, -4.2f));

        // Checkpoint
        var checkpoint = Box("Checkpoint", new Vector2(15.5f, -3.25f), new Vector2(0.4f, 1.5f), new Color(1f, 0.85f, 0.2f), trigger: true);
        checkpoint.AddComponent<Checkpoint>();

        // Moving hazard
        var hazA = Point("Hazard Point A", new Vector2(18f, -3.5f));
        var hazB = Point("Hazard Point B", new Vector2(25f, -3.5f));
        var hazard = Box("Moving Hazard", hazA.position, new Vector2(0.8f, 0.8f), HazardColor, trigger: true);
        var hazRb = hazard.AddComponent<Rigidbody2D>();
        hazRb.bodyType = RigidbodyType2D.Kinematic;
        var mh = hazard.AddComponent<MovingHazard>();
        mh.pointA = hazA;
        mh.pointB = hazB;

        // Pressure switch
        var sw = Box("Pressure Switch", new Vector2(29f, -3.9f), new Vector2(1f, 0.2f), new Color(0.7f, 0.7f, 0.7f), trigger: true);
        sw.AddComponent<PressureSwitch>();

        // Door that blocks the ship until everything is done
        var door = Box("Door", new Vector2(37f, -2f), new Vector2(0.6f, 4f), new Color(0.65f, 0.3f, 1f));
        door.AddComponent<DoorController>();

        // Exit ship
        var ship = Box("Exit Ship", new Vector2(44f, -3f), new Vector2(2.5f, 2f), new Color(0.85f, 0.9f, 1f), trigger: true);
        ship.AddComponent<ExitShip>();

        // Kill zone under the level
        var kill = new GameObject("Kill Zone");
        kill.transform.position = new Vector2(19f, -10f);
        var killCol = kill.AddComponent<BoxCollider2D>();
        killCol.size = new Vector2(120f, 1f);
        killCol.isTrigger = true;
        kill.AddComponent<KillZone>();

        // UI
        var canvas = MakeCanvas();
        MakeEventSystem();

        var ui = canvas.gameObject.AddComponent<GameUI>();
        ui.crystalText = MakeText(canvas.transform, "Crystal Text", "Crystals: 0/3", 40,
            new Vector2(0, 1), new Vector2(30, -20), new Vector2(800, 60), TextAlignmentOptions.TopLeft);
        ui.objectiveText = MakeText(canvas.transform, "Objective Text", "Objective:", 32,
            new Vector2(0, 1), new Vector2(30, -80), new Vector2(1200, 60), TextAlignmentOptions.TopLeft);

        // Win screen
        var winScreen = canvas.gameObject.AddComponent<WinScreen>();
        var winPanel = Panel(canvas.transform, "Win Panel", new Color(0f, 0f, 0f, 0.8f));
        MakeText(winPanel.transform, "Title", "MISSION COMPLETE!", 90,
            new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(1400, 140), TextAlignmentOptions.Center);
        MakeText(winPanel.transform, "Subtitle", "You made it back to your ship.", 40,
            new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(1400, 80), TextAlignmentOptions.Center);
        var playAgain = MakeButton(winPanel.transform, "Play Again Button", "Play Again", new Vector2(0, -60));
        var winQuit = MakeButton(winPanel.transform, "Quit Button", "Quit", new Vector2(0, -170));
        UnityEventTools.AddPersistentListener(playAgain.onClick, new UnityAction(winScreen.PlayAgain));
        UnityEventTools.AddPersistentListener(winQuit.onClick, new UnityAction(winScreen.QuitGame));
        winScreen.winPanel = winPanel;
        gm.winScreen = winScreen;
        winPanel.SetActive(false);

        // Pause menu
        var pause = canvas.gameObject.AddComponent<PauseManager>();
        var pausePanel = Panel(canvas.transform, "Pause Panel", new Color(0f, 0f, 0f, 0.7f));
        MakeText(pausePanel.transform, "Title", "PAUSED", 90,
            new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(1000, 140), TextAlignmentOptions.Center);
        var resume = MakeButton(pausePanel.transform, "Resume Button", "Resume", new Vector2(0, 0));
        var restart = MakeButton(pausePanel.transform, "Restart Button", "Restart", new Vector2(0, -110));
        UnityEventTools.AddPersistentListener(resume.onClick, new UnityAction(pause.Resume));
        UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(pause.Restart));
        pause.pauseMenu = pausePanel;
        pausePanel.SetActive(false);

        string path = SceneFolder + "/MainLevel.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    // ---------- Menu scene ----------

    static string BuildMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera();

        var canvas = MakeCanvas();
        MakeEventSystem();

        var menu = canvas.gameObject.AddComponent<MainMenu>();
        menu.gameSceneName = "MainLevel";

        MakeText(canvas.transform, "Title", "LOST IN SPACE", 120,
            new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1600, 180), TextAlignmentOptions.Center);
        MakeText(canvas.transform, "Subtitle", "Find your way back to Earth", 40,
            new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(1400, 80), TextAlignmentOptions.Center);

        var play = MakeButton(canvas.transform, "Play Button", "Play", new Vector2(0, -20));
        var quit = MakeButton(canvas.transform, "Quit Button", "Quit", new Vector2(0, -130));
        UnityEventTools.AddPersistentListener(play.onClick, new UnityAction(menu.PlayGame));
        UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(menu.QuitGame));

        string path = SceneFolder + "/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    // ---------- World helpers ----------

    static Camera MakeCamera()
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = SpaceColor;
        camGo.AddComponent<AudioListener>();
        return cam;
    }

    // A sprite with no collider
    static GameObject MakeSpriteObject(string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = color;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = size;
        return go;
    }

    // A sprite with a matching box collider. Scale stays 1 so parenting
    // the player to a platform never stretches the player.
    static GameObject Box(string name, Vector2 pos, Vector2 size, Color color, bool trigger = false)
    {
        var go = MakeSpriteObject(name, pos, size, color);
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
        col.isTrigger = trigger;
        return go;
    }

    static Transform Point(string name, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        return go.transform;
    }

    static void Parent(GameObject parent, GameObject child)
    {
        child.transform.SetParent(parent.transform, true);
    }

    static void MakeCrystal(GameObject parent, Vector2 pos)
    {
        var c = Box("Crystal", pos, new Vector2(0.5f, 0.5f), CrystalColor, trigger: true);
        c.transform.rotation = Quaternion.Euler(0, 0, 45f);
        c.AddComponent<Crystal>();
        Parent(parent, c);
    }

    static void MakeMovingPlatform(string name, Vector2 a, Vector2 b)
    {
        var pointA = Point(name + " Point A", a);
        var pointB = Point(name + " Point B", b);
        var plat = Box(name, a, new Vector2(2f, 0.4f), new Color(0.5f, 0.75f, 1f));
        var rb = plat.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var mp = plat.AddComponent<MovingPlatform>();
        mp.pointA = pointA;
        mp.pointB = pointB;
    }

    // ---------- UI helpers ----------

    static Canvas MakeCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    static void MakeEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static TextMeshProUGUI MakeText(Transform parent, string name, string text, int size,
        Vector2 anchor, Vector2 pos, Vector2 box, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        return t;
    }

    static GameObject Panel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    static Button MakeButton(Transform parent, string name, string label, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360, 90);
        go.GetComponent<Image>().color = new Color(0.25f, 0.22f, 0.45f);

        var t = MakeText(go.transform, "Label", label, 40, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(360, 90), TextAlignmentOptions.Center);
        t.raycastTarget = false;

        return go.GetComponent<Button>();
    }
}
#endif
