#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Background music is long, so stream it from disk instead of loading the whole song into memory.
public class MusicImporter : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith("Assets/Audio/Music/")) return;

        var importer = (AudioImporter)assetImporter;
        AudioImporterSampleSettings s = importer.defaultSampleSettings;
        s.loadType = AudioClipLoadType.Streaming;
        s.compressionFormat = AudioCompressionFormat.Vorbis;
        s.quality = 0.7f;
        importer.defaultSampleSettings = s;
        importer.loadInBackground = true;
    }
}
#endif
