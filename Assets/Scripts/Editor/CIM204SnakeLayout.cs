using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204SnakeLayout
{
    static CIM204SnakeLayout() => EditorApplication.delayCall += Apply;
    [MenuItem("CIM204/Apply Snake Puzzle Layout")]
    private static void Apply()
    {
        const string marker="Library/CIM204SnakeLayout.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var puzzle=Object.FindAnyObjectByType<FillGridPuzzle>();
        if(puzzle==null) return;
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        if(sprite==null) return;
        puzzle.Build(sprite);
        EditorUtility.SetDirty(puzzle);
        EditorSceneManager.MarkSceneDirty(puzzle.gameObject.scene);
        if(EditorSceneManager.SaveScene(puzzle.gameObject.scene)) File.WriteAllText(marker,"6x6 snake-reference board with 28 playable tiles.");
    }
}
