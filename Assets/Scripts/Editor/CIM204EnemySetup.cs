using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class CIM204EnemySetup
{
    private const string Marker = "Library/CIM204EnemySetup.complete";
    static CIM204EnemySetup() => EditorApplication.delayCall += TrySetup;

    [MenuItem("CIM204/Create Triangle Enemy and Health")]
    public static void TrySetup()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += TrySetup;
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        CollectibleHUD collectibleHUD = Object.FindFirstObjectByType<CollectibleHUD>();
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (collectibleHUD == null || player == null) return;

        HealthHUD health = Object.FindFirstObjectByType<HealthHUD>();
        if (health == null)
        {
            Text original = collectibleHUD.GetComponentInChildren<Text>(true);
            if (original == null) throw new System.Exception("Cannot find the existing HUD text.");
            GameObject healthObject = Object.Instantiate(original.gameObject, original.transform.parent);
            healthObject.name = "Health Count";
            Undo.RegisterCreatedObjectUndo(healthObject, "Create Health HUD");
            Text healthText = healthObject.GetComponent<Text>();
            RectTransform rect = healthText.rectTransform;
            rect.anchoredPosition += new Vector2(0, -Mathf.Max(40, original.fontSize * 1.5f));
            health = healthObject.AddComponent<HealthHUD>();
            health.SetLabel(healthText);
        }

        const string spritePath = "Assets/Sprites/TriangleEnemy.png";
        const string prefabPath = "Assets/Prefabs/Triangle Enemy.prefab";
        Directory.CreateDirectory("Assets/Sprites");
        Directory.CreateDirectory("Assets/Prefabs");
        Texture2D texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[128 * 128];
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float halfWidth = (127f - (y + 0.5f)) * 0.5f;
                float alpha = Mathf.Clamp01(halfWidth - Mathf.Abs(x + 0.5f - 64f));
                pixels[y * 128 + x] = new Color(1f, 0.2f, 0.2f, alpha);
            }
        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(spritePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 128;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            GameObject template = new GameObject("Triangle Enemy");
            template.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            PolygonCollider2D trigger = template.AddComponent<PolygonCollider2D>();
            trigger.isTrigger = true;
            trigger.SetPath(0, new Vector2[] { new Vector2(-0.49f, -0.5f), new Vector2(0.49f, -0.5f), new Vector2(0, 0.49f) });
            template.AddComponent<TriangleEnemy>();
            prefab = PrefabUtility.SaveAsPrefabAsset(template, prefabPath);
            Object.DestroyImmediate(template);
        }
        GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        Undo.RegisterCreatedObjectUndo(enemy, "Create Triangle Enemy");
        enemy.transform.position = player.transform.position + new Vector3(0, 2, 0);
        Selection.activeGameObject = enemy;
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new System.Exception("Could not save enemy scene setup.");
        File.WriteAllText(Marker, "Triangle Enemy prefab and Health HUD saved.");
        Debug.Log("CIM204: Triangle Enemy prefab created. Health starts at 3 and decreases once per contact, down to 0.");
    }
}
