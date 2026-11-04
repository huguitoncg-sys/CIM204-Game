using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204RestoreCastleLayout
{
    const string Marker = "Library/CIM204RestoreCastleLayout.complete";
    static CIM204RestoreCastleLayout() => EditorApplication.delayCall += Apply;
    [MenuItem("CIM204/Restore Castle Map Layout")]
    static void Apply()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var map = Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if (map == null) return;
        var importer = AssetImporter.GetAtPath("Assets/Resources/CastleTiles.png") as TextureImporter;
        if (importer == null) return;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        if (settings.spriteMeshType != SpriteMeshType.FullRect)
        {
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().ToArray();
        var floor = sprites.FirstOrDefault(s => s.name == "Floor");
        var brick = sprites.FirstOrDefault(s => s.name == "Brick");
        if (floor == null || brick == null) return;
        int floors = 0, walls = 0;
        foreach (var original in map.GetComponentsInChildren<SpriteRenderer>(true))
        {
            bool wall = original.name.StartsWith("Wall ");
            if (!wall && !original.name.StartsWith("Floor ")) continue;
            if (original.sprite == null) continue;
            Vector3 scale = original.transform.lossyScale;
            Vector2 size = new Vector2(
                original.sprite.bounds.size.x * Mathf.Abs(scale.x),
                original.sprite.bounds.size.y * Mathf.Abs(scale.y));
            if (size.x <= 0 || size.y <= 0) continue;
            var child = original.transform.Find("Castle Surface");
            if (child == null)
            {
                var obj = new GameObject("Castle Surface");
                Undo.RegisterCreatedObjectUndo(obj, "Restore castle map surface");
                child = obj.transform;
                child.SetParent(original.transform, false);
                obj.AddComponent<SpriteRenderer>();
            }
            child.localPosition = original.sprite.bounds.center;
            child.localRotation = Quaternion.identity;
            child.localScale = new Vector3(map.cellSize / Mathf.Abs(scale.x), map.cellSize / Mathf.Abs(scale.y), 1);
            var renderer = child.GetComponent<SpriteRenderer>();
            renderer.sprite = wall ? brick : floor;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = size / map.cellSize;
            renderer.sharedMaterial = original.sharedMaterial;
            renderer.color = wall ? Color.white : new Color(.86f, .91f, .97f);
            renderer.sortingLayerID = original.sortingLayerID;
            renderer.sortingOrder = original.sortingOrder;
            renderer.enabled = true;
            original.enabled = false;
            EditorUtility.SetDirty(original);
            EditorUtility.SetDirty(renderer);
            if (wall) walls++; else floors++;
        }
        if (floors == 0 || walls == 0)
        { Debug.LogError("Castle repair could not locate original map surfaces."); return; }
        // Retire the failed replacement meshes, preserving all original colliders.
        var environment = map.transform.Find("Castle Environment");
        if (environment != null)
            foreach (var mesh in environment.GetComponentsInChildren<MeshRenderer>())
            { mesh.enabled = false; EditorUtility.SetDirty(mesh); }
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if (EditorSceneManager.SaveScene(map.gameObject.scene))
        {
            File.WriteAllText(Marker, "Restored " + floors + " floor pieces and " + walls + " wall pieces.");
            Debug.Log("Castle map restored from original layout: " + floors + " floor pieces, " + walls + " wall pieces.");
        }
    }
}

