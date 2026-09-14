using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CIM204SquareSetup
{
    private const string Marker = "Library/CIM204SquareSetup.complete";

    static CIM204SquareSetup()
    {
        EditorApplication.delayCall += TrySetup;
    }

    private static void TrySetup()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += TrySetup;
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        CreateSquare();
    }

    [MenuItem("CIM204/Create Moving Square")]
    private static void CreateSquare()
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.GetComponent<PlayerMovement>() != null) return;

        bool wasDirty = scene.isDirty;
        const string spritePath = "Assets/Sprites/PlayerSquare.png";
        Directory.CreateDirectory("Assets/Sprites");
        if (!File.Exists(spritePath))
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(spritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        GameObject player = new GameObject("Player Square");
        SceneManager.MoveGameObjectToScene(player, scene);
        Undo.RegisterCreatedObjectUndo(player, "Create Moving Square");
        player.transform.position = Vector3.zero;
        player.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        player.AddComponent<PlayerMovement>();
        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(scene);
        if (!wasDirty && !string.IsNullOrEmpty(scene.path)) EditorSceneManager.SaveScene(scene);
        File.WriteAllText(Marker, "Created Player Square with PlayerMovement.");
        Debug.Log("CIM204: Player Square created. Press Play and use WASD or arrow keys.");
    }
}
