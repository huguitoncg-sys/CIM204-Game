using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204SolidWallSides
{
    static CIM204SolidWallSides() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204SolidWallSides.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(map==null) return;
        var root=map.transform.Find("Castle Vertical Wall Edges");
        var brick=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Brick");
        if(root==null || brick==null) return;
        int count=0;
        foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>())
        {
            if(renderer.name!="Vertical Wall Core") continue;
            // Fill the entire original wall footprint, keeping the narrow stone border above it.
            renderer.sprite=brick;
            renderer.color=Color.white;
            renderer.drawMode=SpriteDrawMode.Tiled;
            renderer.tileMode=SpriteTileMode.Continuous;
            renderer.size=Vector2.one;
            EditorUtility.SetDirty(renderer);
            count++;
        }
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))
            File.WriteAllText(marker,"Filled "+count+" vertical wall cores with matching brick.");
    }
}
