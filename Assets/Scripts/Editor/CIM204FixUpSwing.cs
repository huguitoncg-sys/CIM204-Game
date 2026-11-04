using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204FixUpSwing
{
    static CIM204FixUpSwing()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204FixUpSwing.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var player=Object.FindAnyObjectByType<PlayerCharacterAnimation>();
        var sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Kurenai_Updated_Sprite.png").OfType<Sprite>().ToDictionary(s=>s.name);
        if(player==null || !sprites.ContainsKey("Swing_34"))return;
        System.Func<int,Sprite> frame=i=>sprites["Swing_"+i.ToString("D2")];
        player.ConfigureSwings(Enumerable.Range(18,4).Select(frame).ToArray(),
            Enumerable.Range(22,5).Select(frame).ToArray(),
            Enumerable.Range(27,3).Select(frame).ToArray(),
            Enumerable.Range(30,5).Select(frame).ToArray());
        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        if(EditorSceneManager.SaveScene(player.gameObject.scene))File.WriteAllText(marker,"Up swing uses only back-facing frames 27-29.");
    }
}
