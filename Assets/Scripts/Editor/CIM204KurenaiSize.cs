using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204KurenaiSize
{
    static CIM204KurenaiSize() => EditorApplication.delayCall += Apply;

    [MenuItem("CIM204/Enlarge Kurenai Sprite")]
    private static void Apply()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Apply;
            return;
        }
        var importer = AssetImporter.GetAtPath("Assets/Sprites/Kurenai spritesheet.png") as TextureImporter;
        if (importer == null || Mathf.Approximately(importer.spritePixelsPerUnit, 20f)) return;
        // Enlarge only the art; retain the player's transform and collision dimensions.
        importer.spritePixelsPerUnit = 20f;
        importer.SaveAndReimport();
    }
}


