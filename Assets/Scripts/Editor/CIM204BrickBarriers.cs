using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204BrickBarriers
{
    static CIM204BrickBarriers() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204BrickBarriers.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var encounter=Object.FindAnyObjectByType<FirstRoomEncounter>();
        if(encounter==null || encounter.map==null) return;
        var brick=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Brick");
        if(brick==null) return;
        foreach(var door in encounter.yellowDoors)
        {
            if(door==null) continue;
            var renderer=door.GetComponent<SpriteRenderer>();
            var collider=door.GetComponent<BoxCollider2D>();
            if(renderer==null || collider==null) continue;
            // Use exactly the existing blocked footprint and the same pixel scale as adjoining walls.
            Vector2 scale=door.transform.lossyScale;
            Vector2 size=Vector2.Scale(collider.size,scale);
            Vector2 offset=Vector2.Scale(collider.offset,scale);
            float tileScale=encounter.map.cellSize;
            Vector3 parent=door.transform.parent.lossyScale;
            door.transform.localScale=new Vector3(tileScale/parent.x,tileScale/parent.y,1);
            collider.size=size/tileScale;collider.offset=offset/tileScale;
            renderer.sprite=brick;
            renderer.drawMode=SpriteDrawMode.Tiled;
            renderer.tileMode=SpriteTileMode.Continuous;
            renderer.size=size/tileScale;
            renderer.color=Color.white;
            var oldDoor=door.GetComponent<CastleDoor>();
            if(oldDoor!=null) Object.DestroyImmediate(oldDoor);
            EditorUtility.SetDirty(renderer);EditorUtility.SetDirty(collider);EditorUtility.SetDirty(door.transform);
        }
        EditorSceneManager.MarkSceneDirty(encounter.gameObject.scene);
        if(EditorSceneManager.SaveScene(encounter.gameObject.scene))File.WriteAllText(marker,"Doors replaced with matching removable brick walls.");
    }
}
