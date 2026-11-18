using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204DaimaSize
{
    static CIM204DaimaSize()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204DaimaSize.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var boss=Object.FindAnyObjectByType<DaimaBoss>();
        if(boss==null)return;
        Undo.RecordObject(boss.transform,"Double Daima size");
        boss.transform.localScale=new Vector3(2,2,1);
        EditorUtility.SetDirty(boss.transform);
        EditorSceneManager.MarkSceneDirty(boss.gameObject.scene);
        if(EditorSceneManager.SaveScene(boss.gameObject.scene))File.WriteAllText(marker,"Daima scaled to 2x.");
    }
}
