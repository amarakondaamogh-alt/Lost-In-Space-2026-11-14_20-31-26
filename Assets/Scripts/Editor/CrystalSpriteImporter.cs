#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Makes the pixel-art sprites import sharp (no blur, no compression) at the right size:
//   Crystal.png - about 0.9 units tall
//   Spike.png   - about 0.56 units tall
public class CrystalSpriteImporter : AssetPostprocessor
{
    public const string CrystalSpritePath = "Assets/Sprites/Crystal.png";
    public const string SpikeSpritePath = "Assets/Sprites/Resources/Spike.png";
    public const string RocketSpritePath = "Assets/Sprites/Spaceship/Rocket.png";

    void OnPreprocessTexture()
    {
        float pixelsPerUnit;
        if (assetPath == CrystalSpritePath) pixelsPerUnit = 640f;
        else if (assetPath == SpikeSpritePath) pixelsPerUnit = 800f;
        else if (assetPath == RocketSpritePath) pixelsPerUnit = 10f;   // 18x30 pixels -> 1.8 x 3 units
        else return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
    }
}
#endif
