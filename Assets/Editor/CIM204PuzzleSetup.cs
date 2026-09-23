using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CIM204PuzzleSetup
{
    private const string Marker = "Library/CIM204PuzzleSetup.complete";
    static CIM204PuzzleSetup() => EditorApplication.delayCall += Setup;

    [MenuItem("CIM204/Create Fill Grid Puzzle")]
    public static void Setup()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Setup;
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        if (Object.FindFirstObjectByType<FillGridPuzzle>() != null) return;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        if (sprite == null) return;
        SpriteRenderer farthest = null;
        foreach (SpriteRenderer candidate in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (candidate.gameObject.scene != scene || !candidate.name.StartsWith("Background Rectangle")) continue;
            if (farthest == null || candidate.bounds.max.x > farthest.bounds.max.x) farthest = candidate;
        }
        if (farthest == null) return;
        GameObject background = new GameObject("Background Rectangle - Fill Grid Puzzle");
        Undo.RegisterCreatedObjectUndo(background, "Add Fill Grid Puzzle Room");
        SceneManager.MoveGameObjectToScene(background, scene);
        background.transform.position = new Vector3(farthest.bounds.max.x + 11, farthest.bounds.center.y, 0);
        background.transform.localScale = new Vector3(18, 10, 1);
        SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.09f, 0.16f, 0.23f);
        renderer.sortingOrder = -20;
        background.AddComponent<RoomArea>();
        background.AddComponent<FillGridPuzzle>().Build(sprite);
        // Keep the existing player visible over the puzzle tiles.
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            Undo.RecordObject(playerSprite, "Draw Player Over Puzzle");
            playerSprite.sortingOrder = 20;
            EditorUtility.SetDirty(playerSprite);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new System.Exception("Could not save puzzle room.");
        File.WriteAllText(Marker, "Created 7 by 6 fill grid puzzle with top-center start and bottom-center obstacle.");
        Debug.Log("CIM204: Fill Grid Puzzle room created to the right of the rightmost room. Enter to play; R restarts, E releases the player.");
    }
}
