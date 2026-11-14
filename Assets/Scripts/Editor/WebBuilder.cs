#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Menu: Lost in Space > Build for itch.io (Web)
//
// Makes a web (WebGL) version of the game in the "Builds/WebGL" folder, ready to zip and
// upload to itch.io. The Space Cadet song is left out (it's copyrighted), so the uploaded
// game plays its built-in tune. Your own copy keeps playing the song.
public static class WebBuilder
{
    const string OutputFolder = "Builds/WebGL";
    const string SongInResources = "Assets/Audio/Music/Resources/SpaceCadet.mp3";
    const string SongParked = "Assets/Audio/Music/SpaceCadet.mp3";

    [MenuItem("Lost in Space/Build for itch.io (Web)")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Lost in Space", "Stop Play mode first, then try again.", "OK");
            return;
        }
        if (!EditorSceneManagerSave()) return;

        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Lost in Space", "No scenes in Build Settings.", "OK");
            return;
        }

        // Web settings that work well on itch.io.
        PlayerSettings.productName = "Lost in Space";
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 540;
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled; // itch.io serves it as-is
        PlayerSettings.WebGL.decompressionFallback = false;

        // Take the song out of Resources so it isn't packed into the build.
        bool parked = false;
        if (File.Exists(SongInResources))
            parked = string.IsNullOrEmpty(AssetDatabase.MoveAsset(SongInResources, SongParked));

        BuildReport report;
        try
        {
            if (Directory.Exists(OutputFolder)) Directory.Delete(OutputFolder, true);
            report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputFolder,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
        }
        finally
        {
            if (parked) AssetDatabase.MoveAsset(SongParked, SongInResources);
        }

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[Lost in Space] Web build finished: " + Path.GetFullPath(OutputFolder));
            EditorUtility.DisplayDialog("Lost in Space",
                "Web build finished!\n\nIt's in the Builds/WebGL folder of your project. " +
                "Go back to Claude and say it's done.", "OK");
        }
        else
        {
            Debug.LogError("[Lost in Space] Web build failed: " + report.summary.result + " (" + report.summary.totalErrors + " errors). See the Console.");
            EditorUtility.DisplayDialog("Lost in Space", "The web build failed. Check the red messages in the Console.", "OK");
        }
    }

    static bool EditorSceneManagerSave()
    {
        return UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
    }
}
#endif
