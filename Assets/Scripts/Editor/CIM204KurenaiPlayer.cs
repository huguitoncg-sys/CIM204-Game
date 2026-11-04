using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204KurenaiPlayer
{
    private const string AssetPath = "Assets/Sprites/Kurenai spritesheet.png";
    private const string Marker = "Library/CIM204KurenaiPlayer.complete";
    static CIM204KurenaiPlayer() => EditorApplication.delayCall += Apply;
    [MenuItem("CIM204/Apply Kurenai Player")]
    private static void Apply()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var player = Object.FindAnyObjectByType<PlayerMovement>();
        var importer = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
        if (player == null || importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 20;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var slices = new SpriteRect[19];
        for (int i = 0; i < slices.Length; i++)
            slices[i] = new SpriteRect {
                name = "Kurenai_" + i.ToString("D2"),
                rect = new Rect((i % 4) * 32, 160 - (i / 4 + 1) * 32, 32, 32),
                alignment = SpriteAlignment.Center,
                spriteID = GUID.Generate(),
                pivot = new Vector2(0.5f, 0.5f)
            };
        provider.SetSpriteRects(slices);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(slices.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();
        var frames = AssetDatabase.LoadAllAssetsAtPath(AssetPath).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (frames.Length != 19) { Debug.LogError("Kurenai: expected 19 sprite frames."); return; }
        Undo.RecordObject(player.GetComponent<SpriteRenderer>(), "Replace player appearance");
        var animation = player.GetComponent<PlayerCharacterAnimation>();
        if (animation == null) animation = Undo.AddComponent<PlayerCharacterAnimation>(player.gameObject);
        animation.Configure(frames);
        EditorUtility.SetDirty(animation);
        EditorUtility.SetDirty(player.GetComponent<SpriteRenderer>());
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        if (EditorSceneManager.SaveScene(player.gameObject.scene))
            File.WriteAllText(Marker, "Kurenai player: idle and four-direction walking.");
    }
}




