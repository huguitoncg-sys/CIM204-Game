using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class CIM204CollectibleSetup
{
    private const string Marker = "Library/CIM204CollectibleSetup.complete";
    static CIM204CollectibleSetup() => EditorApplication.delayCall += TrySetup;

    public static void SetupBatch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        TrySetup();
        if (!File.Exists(Marker)) throw new System.Exception("Collectible scene setup did not complete.");
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
        PlayerMovement player = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.TryGetComponent(out PlayerMovement movement)) player = movement;
        if (player == null) return;

        bool wasDirty = scene.isDirty;
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body == null) body = Undo.AddComponent<Rigidbody2D>(player.gameObject);
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (player.GetComponent<BoxCollider2D>() == null) Undo.AddComponent<BoxCollider2D>(player.gameObject);
        EditorUtility.SetDirty(body);

        const string spritePath = "Assets/Sprites/CircleCollectible.png";
        Directory.CreateDirectory("Assets/Sprites");
        Texture2D texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[128 * 128];
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64, 64));
                pixels[y * 128 + x] = new Color(1f, 0.78f, 0.12f, Mathf.Clamp01(63.5f - distance));
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

        GameObject canvasObject = new GameObject("Collectible HUD", typeof(Canvas), typeof(CanvasScaler));
        SceneManager.MoveGameObjectToScene(canvasObject, scene);
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Collectible HUD");
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        GameObject textObject = new GameObject("Collectible Count", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(20, -20);
        rect.sizeDelta = new Vector2(320, 50);
        Text label = textObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 30;
        label.fontStyle = FontStyle.Bold;
        label.color = Color.white;
        label.alignment = TextAnchor.UpperLeft;
        label.raycastTarget = false;
        CollectibleHUD hud = canvasObject.AddComponent<CollectibleHUD>();
        hud.SetLabel(label);

        GameObject circle = new GameObject("circle collectible");
        SceneManager.MoveGameObjectToScene(circle, scene);
        Undo.RegisterCreatedObjectUndo(circle, "Create Circle Collectible");
        circle.transform.position = player.transform.position + new Vector3(2.5f, 0, 0);
        circle.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        CircleCollider2D trigger = circle.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.49f;
        circle.AddComponent<CircleCollectible>().SetHUD(hud);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!wasDirty) EditorSceneManager.SaveScene(scene);
        File.WriteAllText(Marker, "Circle and HUD created.");
        Debug.Log("CIM204: Circle collectible and HUD created; count starts at 0 and becomes 1 on contact.");
    }
}
