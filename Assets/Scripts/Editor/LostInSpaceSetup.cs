#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

// Menu: Lost in Space > Set Up Everything
//
// Connects the game scripts to the objects in all 10 level scenes (by object name),
// adds managers / HUD / pause / win screens, builds the MainMenu, WinScene and Credits
// screens, and puts every scene into Build Settings in the right order.
// It is safe to run more than once.
public static class LostInSpaceSetup
{
    const string SceneFolder = "Assets/Scenes";
    const string MaterialPath = "Assets/Materials/NoFriction.physicsMaterial2D";
    const string AstronautPrefabPath = "Assets/Prefabs/Player/AstronautPrefab.prefab";

    static readonly string[] LevelScenes =
    {
        "MainLevel", "Level02", "Level03", "Level04", "Level05",
        "Level06", "Level07", "Level08", "Level09", "Level10"
    };

    static readonly Color SpaceColor = new Color(0.04f, 0.03f, 0.12f);
    static readonly Color ButtonColor = new Color(0.25f, 0.22f, 0.45f);

    static readonly List<string> report = new List<string>();
    static PhysicsMaterial2D noFriction;

    [MenuItem("Lost in Space/Set Up Everything (levels, menus, build settings)")]
    public static void SetupAll()
    {
        report.Clear();

        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Lost in Space", "Stop Play mode first, then run this again.", "OK");
            return;
        }

        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            EditorUtility.DisplayDialog("Lost in Space",
                "TextMeshPro resources are missing.\n\nGo to Window > TextMeshPro > Import TMP Essential Resources, " +
                "click Import, then run this again.", "OK");
            return;
        }

        var missing = LevelScenes.Where(n => !File.Exists(SceneFolder + "/" + n + ".unity")).ToList();
        if (missing.Count > 0)
        {
            EditorUtility.DisplayDialog("Lost in Space",
                "These level scenes were not found in Assets/Scenes:\n" + string.Join(", ", missing), "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Lost in Space",
            "This will connect the scripts in all 10 levels, rebuild the MainMenu, WinScene and Credits scenes, " +
            "and set up Build Settings.\n\nA backup of your original scenes and scripts is in the " +
            "'_backup_before_fix' folder next to your project. Continue?", "Set up", "Cancel"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        try
        {
            noFriction = GetNoFrictionMaterial();

            for (int i = 0; i < LevelScenes.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Lost in Space", "Setting up " + LevelScenes[i], (float)i / (LevelScenes.Length + 3));
                WireLevel(LevelScenes[i], i + 1);
            }

            EditorUtility.DisplayProgressBar("Lost in Space", "Building menu screens", 0.9f);
            BuildMenuScene();
            BuildWinScene();
            BuildCreditsScene();

            SetBuildSettings();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(SceneFolder + "/MainMenu.unity", OpenSceneMode.Single);

        var sb = new StringBuilder();
        sb.AppendLine("All done! 10 levels, MainMenu, WinScene and Credits are set up and in Build Settings.");
        sb.AppendLine();
        sb.AppendLine("Controls: A/D or arrows to move, Space (or W/Up) to jump, Esc to pause.");
        sb.AppendLine("Press Play on the MainMenu scene to test the whole game.");
        if (report.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Things to check:");
            foreach (string r in report) sb.AppendLine(" - " + r);
        }

        Debug.Log("[Lost in Space] Setup finished.\n" + sb);
        EditorUtility.DisplayDialog("Lost in Space", sb.ToString(), "OK");
    }

    // ---------------------------------------------------------------- levels

    static void WireLevel(string sceneName, int levelNumber)
    {
        string path = SceneFolder + "/" + sceneName + ".unity";
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        string tag = "[" + sceneName + "] ";

        // Remove generated objects so running the tool again never duplicates them.
        foreach (GameObject go in AllObjects(scene))
            if (go != null && (go.name == "HUD Canvas" || go.name == "Kill Zone"))
                Object.DestroyImmediate(go);

        List<GameObject> all = AllObjects(scene);

        // ---- Player
        GameObject player = all.FirstOrDefault(g => g.CompareTag("Player"));
        if (player == null)
        {
            var pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }
        if (player == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AstronautPrefabPath);
            if (prefab != null)
            {
                player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                player.transform.position = new Vector3(-8f, 0f, 0f);
                report.Add(tag + "had no player, so I added the Astronaut prefab at (-8, 0). Move it where you want.");
            }
        }
        if (player == null)
        {
            report.Add(tag + "NO PLAYER FOUND and no Astronaut prefab - this level was skipped.");
            EditorSceneManager.SaveScene(scene);
            return;
        }

        player.tag = "Player";

        Rigidbody2D rb = Ensure<Rigidbody2D>(player);
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (rb.gravityScale <= 0f) rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        Collider2D playerCol = player.GetComponent<Collider2D>();
        if (playerCol == null) playerCol = ObjectFactory.AddComponent<BoxCollider2D>(player);
        playerCol.isTrigger = false;
        playerCol.sharedMaterial = noFriction;

        Ensure<PlayerController>(player);

        // ---- Camera
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            var follow = Ensure<CameraFollow>(cam.gameObject);
            follow.player = player.transform;
            cam.transform.position = new Vector3(player.transform.position.x, Mathf.Max(follow.minY, 0f), -10f);
        }
        else
        {
            report.Add(tag + "no camera found.");
        }

        // ---- Level objects (matched by name)
        foreach (GameObject go in Named(all, player, "Crystal"))
        {
            Ensure<Crystal>(go);
            SetCollider(go, true);
        }
        foreach (GameObject go in Named(all, player, "Switch"))
        {
            Ensure<PressureSwitch>(go);
            SetCollider(go, true);
        }
        foreach (GameObject go in Named(all, player, "Door"))
        {
            Ensure<DoorController>(go);
            SetCollider(go, false);
        }
        foreach (GameObject go in Named(all, player, "Spaceship", "ExitShip"))
        {
            Ensure<ExitShip>(go);
            SetCollider(go, true);
        }
        foreach (GameObject go in Named(all, player, "BouncePad"))
        {
            Ensure<BouncePad>(go);
            SetCollider(go, true);
        }
        foreach (GameObject go in Named(all, player, "Hazard"))
        {
            Ensure<MovingHazard>(go);
            SetCollider(go, true);
        }
        foreach (GameObject go in Named(all, player, "Checkpoint"))
        {
            Ensure<Checkpoint>(go);
            SetCollider(go, true);
        }

        int crystalCount = Object.FindObjectsByType<Crystal>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        if (crystalCount == 0) report.Add(tag + "no objects named 'Crystal...' found.");
        if (Object.FindFirstObjectByType<PressureSwitch>() == null) report.Add(tag + "no object named 'Switch...' - the door can never unlock.");
        if (Object.FindFirstObjectByType<DoorController>() == null) report.Add(tag + "no object named 'Door...' found.");
        if (Object.FindFirstObjectByType<ExitShip>() == null) report.Add(tag + "no object named 'Spaceship...' - the level can't be finished.");

        // ---- Managers
        var existingGm = Object.FindFirstObjectByType<GameManager>();
        GameObject managers = existingGm != null ? existingGm.gameObject : new GameObject("Managers");
        GameManager gm = existingGm != null ? existingGm : managers.AddComponent<GameManager>();
        if (crystalCount > 0) gm.crystalsRequired = crystalCount;
        if (Object.FindFirstObjectByType<RespawnManager>() == null) managers.AddComponent<RespawnManager>();

        SoundManager sm = Object.FindFirstObjectByType<SoundManager>();
        if (sm == null) sm = managers.AddComponent<SoundManager>();
        if (sm.audioSource == null)
        {
            AudioSource src = sm.GetComponent<AudioSource>();
            if (src == null) src = sm.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sm.audioSource = src;
        }

        // ---- Kill zone under the level (falling off = respawn)
        AddKillZone();

        // ---- UI
        if (Object.FindFirstObjectByType<EventSystem>() == null) MakeEventSystem();
        BuildHud(gm, levelNumber, sceneName == LevelScenes[LevelScenes.Length - 1]);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void AddKillZone()
    {
        var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        float minY = 0f, minX = 0f, maxX = 0f;
        if (renderers.Length > 0)
        {
            minY = renderers.Min(r => r.bounds.min.y);
            minX = renderers.Min(r => r.bounds.min.x);
            maxX = renderers.Max(r => r.bounds.max.x);
        }

        var kill = new GameObject("Kill Zone");
        kill.transform.position = new Vector3((minX + maxX) * 0.5f, minY - 8f, 0f);
        var col = kill.AddComponent<BoxCollider2D>();
        col.size = new Vector2((maxX - minX) + 200f, 2f);
        col.isTrigger = true;
        kill.AddComponent<KillZone>();
    }

    static void BuildHud(GameManager gm, int levelNumber, bool isLastLevel)
    {
        Canvas canvas = MakeCanvas("HUD Canvas");

        var ui = canvas.gameObject.AddComponent<GameUI>();
        ui.crystalText = MakeText(canvas.transform, "Crystal Text", "Crystals: 0/" + gm.crystalsRequired, 40,
            new Vector2(0, 1), new Vector2(30, -20), new Vector2(800, 60), TextAlignmentOptions.TopLeft);
        ui.objectiveText = MakeText(canvas.transform, "Objective Text", "Objective:", 32,
            new Vector2(0, 1), new Vector2(30, -80), new Vector2(1500, 60), TextAlignmentOptions.TopLeft);
        MakeText(canvas.transform, "Level Text", "Level " + levelNumber + " / " + LevelScenes.Length, 40,
            new Vector2(1, 1), new Vector2(-30, -20), new Vector2(600, 60), TextAlignmentOptions.TopRight);

        var loader = canvas.gameObject.AddComponent<SceneLoader>();

        // Win panel
        var win = canvas.gameObject.AddComponent<WinScreen>();
        GameObject winPanel = MakePanel(canvas.transform, "Win Panel", new Color(0f, 0f, 0f, 0.8f));
        MakeText(winPanel.transform, "Title", isLastLevel ? "MISSION COMPLETE!" : "LEVEL COMPLETE!", 90,
            new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(1400, 140), TextAlignmentOptions.Center);
        Button next = MakeButton(winPanel.transform, "Next Button", isLastLevel ? "Continue" : "Next Level", new Vector2(0, 40));
        Button again = MakeButton(winPanel.transform, "Replay Button", "Replay Level", new Vector2(0, -70));
        Button menu = MakeButton(winPanel.transform, "Menu Button", "Main Menu", new Vector2(0, -180));
        UnityEventTools.AddPersistentListener(next.onClick, new UnityAction(win.NextLevel));
        UnityEventTools.AddPersistentListener(again.onClick, new UnityAction(win.PlayAgain));
        UnityEventTools.AddStringPersistentListener(menu.onClick, new UnityAction<string>(loader.Load), "MainMenu");
        win.winPanel = winPanel;
        gm.winScreen = win;
        EditorUtility.SetDirty(gm);
        winPanel.SetActive(false);

        // Pause panel
        var pause = canvas.gameObject.AddComponent<PauseManager>();
        GameObject pausePanel = MakePanel(canvas.transform, "Pause Panel", new Color(0f, 0f, 0f, 0.7f));
        MakeText(pausePanel.transform, "Title", "PAUSED", 90,
            new Vector2(0.5f, 0.5f), new Vector2(0, 200), new Vector2(1000, 140), TextAlignmentOptions.Center);
        Button resume = MakeButton(pausePanel.transform, "Resume Button", "Resume", new Vector2(0, 40));
        Button restart = MakeButton(pausePanel.transform, "Restart Button", "Restart", new Vector2(0, -70));
        Button pauseMenu = MakeButton(pausePanel.transform, "Menu Button", "Main Menu", new Vector2(0, -180));
        UnityEventTools.AddPersistentListener(resume.onClick, new UnityAction(pause.Resume));
        UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(pause.Restart));
        UnityEventTools.AddStringPersistentListener(pauseMenu.onClick, new UnityAction<string>(loader.Load), "MainMenu");
        pause.pauseMenu = pausePanel;
        pausePanel.SetActive(false);
    }

    // ---------------------------------------------------------------- menu scenes

    static void BuildMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera();
        Canvas canvas = MakeCanvas("Canvas");
        MakeEventSystem();

        var menu = canvas.gameObject.AddComponent<MainMenu>();
        menu.gameSceneName = "MainLevel";

        MakeText(canvas.transform, "Title", "LOST IN SPACE", 120,
            new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1600, 180), TextAlignmentOptions.Center);
        MakeText(canvas.transform, "Subtitle", "Find your way back to your spaceship", 40,
            new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(1400, 80), TextAlignmentOptions.Center);

        Button play = MakeButton(canvas.transform, "Play Button", "Play", new Vector2(0, -20));
        Button quit = MakeButton(canvas.transform, "Quit Button", "Quit", new Vector2(0, -130));
        UnityEventTools.AddPersistentListener(play.onClick, new UnityAction(menu.PlayGame));
        UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(menu.QuitGame));

        EditorSceneManager.SaveScene(scene, SceneFolder + "/MainMenu.unity");
    }

    static void BuildWinScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera();
        Canvas canvas = MakeCanvas("Canvas");
        MakeEventSystem();
        var loader = canvas.gameObject.AddComponent<SceneLoader>();

        MakeText(canvas.transform, "Title", "YOU MADE IT HOME!", 110,
            new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1700, 170), TextAlignmentOptions.Center);
        MakeText(canvas.transform, "Subtitle", "All 10 levels complete. Mission accomplished!", 44,
            new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(1500, 80), TextAlignmentOptions.Center);

        Button credits = MakeButton(canvas.transform, "Credits Button", "Credits", new Vector2(0, -10));
        Button again = MakeButton(canvas.transform, "Play Again Button", "Play Again", new Vector2(0, -120));
        Button menu = MakeButton(canvas.transform, "Menu Button", "Main Menu", new Vector2(0, -230));
        UnityEventTools.AddStringPersistentListener(credits.onClick, new UnityAction<string>(loader.Load), "Credits");
        UnityEventTools.AddStringPersistentListener(again.onClick, new UnityAction<string>(loader.Load), "MainLevel");
        UnityEventTools.AddStringPersistentListener(menu.onClick, new UnityAction<string>(loader.Load), "MainMenu");

        EditorSceneManager.SaveScene(scene, SceneFolder + "/WinScene.unity");
    }

    static void BuildCreditsScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera();
        Canvas canvas = MakeCanvas("Canvas");
        MakeEventSystem();
        var loader = canvas.gameObject.AddComponent<SceneLoader>();

        MakeText(canvas.transform, "Title", "CREDITS", 110,
            new Vector2(0.5f, 0.5f), new Vector2(0, 300), new Vector2(1400, 160), TextAlignmentOptions.Center);
        MakeText(canvas.transform, "Body",
            "LOST IN SPACE\n\nA platformer made in Unity\n\nThanks for playing!", 50,
            new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1400, 420), TextAlignmentOptions.Center);

        Button menu = MakeButton(canvas.transform, "Menu Button", "Main Menu", new Vector2(0, -300));
        UnityEventTools.AddStringPersistentListener(menu.onClick, new UnityAction<string>(loader.Load), "MainMenu");

        EditorSceneManager.SaveScene(scene, SceneFolder + "/Credits.unity");
    }

    static void SetBuildSettings()
    {
        var order = new List<string> { "MainMenu" };
        order.AddRange(LevelScenes);
        order.Add("WinScene");
        order.Add("Credits");

        EditorBuildSettings.scenes = order
            .Select(n => new EditorBuildSettingsScene(SceneFolder + "/" + n + ".unity", true))
            .ToArray();
    }

    // ---------------------------------------------------------------- helpers

    static PhysicsMaterial2D GetNoFrictionMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);
        if (mat == null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MaterialPath));
            mat = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(mat, MaterialPath);
            AssetDatabase.SaveAssets();
        }
        return mat;
    }

    static List<GameObject> AllObjects(Scene scene)
    {
        var list = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                list.Add(t.gameObject);
        return list;
    }

    // Objects whose name starts with any of the given prefixes (ignoring case).
    static List<GameObject> Named(List<GameObject> all, GameObject exclude, params string[] prefixes)
    {
        return all.Where(g => g != null && g != exclude && g.name.Length > 0 &&
                              prefixes.Any(p => g.name.StartsWith(p, System.StringComparison.OrdinalIgnoreCase)))
                  .ToList();
    }

    static T Ensure<T>(GameObject go) where T : Component
    {
        T c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    static void SetCollider(GameObject go, bool trigger)
    {
        Collider2D col = go.GetComponent<Collider2D>();
        if (col == null) col = ObjectFactory.AddComponent<BoxCollider2D>(go); // auto-fits the sprite
        col.isTrigger = trigger;
    }

    static Camera MakeCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, 0f, -10f);
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = SpaceColor;
        go.AddComponent<AudioListener>();
        return cam;
    }

    static Canvas MakeCanvas(string name)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    // Uses the module that matches the project's active input handling.
    static void MakeEventSystem()
    {
        var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        var module = go.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
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
        t.raycastTarget = false;
        return t;
    }

    static GameObject MakePanel(Transform parent, string name, Color color)
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
        rt.sizeDelta = new Vector2(420, 90);
        go.GetComponent<Image>().color = ButtonColor;

        MakeText(go.transform, "Label", label, 44, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(420, 90), TextAlignmentOptions.Center);

        return go.GetComponent<Button>();
    }
}
#endif
