using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204RemoveWallTops
{
    static CIM204RemoveWallTops() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204RemoveWallTops.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(map==null) return;
        var tops=map.transform.Find("Castle Top Walls");
        if(tops!=null) Undo.DestroyObjectImmediate(tops.gameObject);
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))
            File.WriteAllText(marker,"Removed decorative top-wall overlays; regular brick surfaces retained.");
    }
}
