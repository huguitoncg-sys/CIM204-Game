using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204TurquoiseMap
{
    static CIM204TurquoiseMap() => EditorApplication.delayCall += Apply;
    private static void Apply()
    {
        const string marker = "Library/CIM204TurquoiseMap.complete";
        if (File.Exists(marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Apply;
            return;
        }
        var map = Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if (map == null) return;
        foreach (var sprite in map.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!sprite.name.StartsWith("Floor ") && sprite.GetComponent<FillGridPuzzle>() == null) continue;
            Undo.RecordObject(sprite, "Change map floor to turquoise");
            sprite.color = new Color(0.10f, 0.73f, 0.66f);
            EditorUtility.SetDirty(sprite);
        }
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if (EditorSceneManager.SaveScene(map.gameObject.scene)) File.WriteAllText(marker, "Map floors changed to turquoise.");
    }
}
