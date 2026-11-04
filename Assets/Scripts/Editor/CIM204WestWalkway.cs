using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204WestWalkway
{
    static CIM204WestWalkway() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204WestWalkway.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(map==null) return;
        var original=map.GetComponentsInChildren<BoxCollider2D>().FirstOrDefault(c=>c.name=="Wall 18,29");
        var sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().ToDictionary(s=>s.name);
        if(original==null || !sprites.ContainsKey("Brick") || !sprites.ContainsKey("Floor")) return;
        var material=original.sharedMaterial;
        var originalRenderer=original.GetComponent<SpriteRenderer>();
        for(int part=0;part<3;part++)
        {
            bool floor=part==1;
            int start=part==0?18:part==1?20:22;
            int width=part==2?3:2;
            var obj=new GameObject(floor?"West Hall Walkway":"West Divider Brick "+part);
            obj.transform.SetParent(map.transform,false);
            obj.transform.position=map.Cell(start+(width-1)*.5f,29);
            obj.transform.localScale=Vector3.one*map.cellSize;
            var renderer=obj.AddComponent<SpriteRenderer>();
            renderer.sprite=sprites[floor?"Floor":"Brick"];
            renderer.drawMode=SpriteDrawMode.Tiled;renderer.tileMode=SpriteTileMode.Continuous;
            renderer.size=new Vector2(width,1);
            renderer.color=floor?new Color(.86f,.91f,.97f):Color.white;
            renderer.sortingOrder=floor?-10:-5;
            if(originalRenderer!=null) renderer.sharedMaterial=originalRenderer.sharedMaterial;
            if(!floor){var collider=obj.AddComponent<BoxCollider2D>();collider.size=renderer.size;collider.sharedMaterial=material;}
        }
        Undo.DestroyObjectImmediate(original.gameObject);
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene)) File.WriteAllText(marker,"Opened cells 20,29 and 21,29 in the west hallway divider.");
    }
}
