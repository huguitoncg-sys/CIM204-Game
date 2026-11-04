using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204RegularWallTiles
{
    static CIM204RegularWallTiles() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204RegularWallTiles.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(map==null) return;
        var brick=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Brick");
        if(brick==null) return;
        var borders=map.transform.Find("Castle Vertical Wall Edges");
        if(borders!=null) Undo.DestroyObjectImmediate(borders.gameObject);
        int count=0;
        foreach(var original in map.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if(!original.name.StartsWith("Wall ")) continue;
            var surface=original.transform.Find("Castle Surface");
            if(surface==null) continue;
            Vector3 parentScale=original.transform.lossyScale;
            Vector2 worldSize=new Vector2(original.sprite.bounds.size.x*Mathf.Abs(parentScale.x),
                original.sprite.bounds.size.y*Mathf.Abs(parentScale.y));
            surface.localScale=new Vector3(map.cellSize/Mathf.Abs(parentScale.x),map.cellSize/Mathf.Abs(parentScale.y),1);
            var renderer=surface.GetComponent<SpriteRenderer>();
            renderer.sprite=brick;
            renderer.drawMode=SpriteDrawMode.Tiled;
            renderer.tileMode=SpriteTileMode.Continuous;
            renderer.size=worldSize/map.cellSize;
            renderer.color=Color.white;
            renderer.enabled=true;
            original.enabled=false;
            EditorUtility.SetDirty(original);EditorUtility.SetDirty(renderer);EditorUtility.SetDirty(surface);
            count++;
        }
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))
            File.WriteAllText(marker,"Removed vertical border overlays; "+count+" wall surfaces use unstretched regular tiles.");
    }
}
