using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204PuzzleWallTiling
{
    static CIM204PuzzleWallTiling() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204PuzzleWallTiling.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var puzzle=Object.FindAnyObjectByType<BoulderPuzzleRoom>();
        if(puzzle==null || puzzle.map==null) return;
        var brick=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Brick");
        if(brick==null) return;
        int count=0;
        foreach(var original in puzzle.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if(!original.name.StartsWith("Connected Puzzle Wall")) continue;
            Vector3 scale=original.transform.lossyScale;
            var collider=original.GetComponent<BoxCollider2D>();
            if(collider==null) continue;
            Vector2 worldSize=new Vector2(collider.size.x*Mathf.Abs(scale.x),collider.size.y*Mathf.Abs(scale.y));
            var child=original.transform.Find("Tiled Brick Surface");
            if(child==null)
            {
                child=new GameObject("Tiled Brick Surface").transform;
                child.SetParent(original.transform,false);
                child.gameObject.AddComponent<SpriteRenderer>();
            }
            child.localPosition=collider.offset;
            child.localScale=new Vector3(puzzle.map.cellSize/Mathf.Abs(scale.x),puzzle.map.cellSize/Mathf.Abs(scale.y),1);
            var renderer=child.GetComponent<SpriteRenderer>();
            renderer.sprite=brick;
            renderer.drawMode=SpriteDrawMode.Tiled;
            renderer.tileMode=SpriteTileMode.Continuous;
            renderer.size=worldSize/puzzle.map.cellSize;
            renderer.color=Color.white;
            renderer.sharedMaterial=original.sharedMaterial;
            renderer.sortingLayerID=original.sortingLayerID;
            renderer.sortingOrder=original.sortingOrder;
            original.enabled=false;
            EditorUtility.SetDirty(original);
            EditorUtility.SetDirty(renderer);
            count++;
        }
        if(count==0) return;
        EditorSceneManager.MarkSceneDirty(puzzle.gameObject.scene);
        if(EditorSceneManager.SaveScene(puzzle.gameObject.scene))
            File.WriteAllText(marker,"Fixed "+count+" puzzle wall strips with regular-scale repeating brick tiles.");
    }
}
