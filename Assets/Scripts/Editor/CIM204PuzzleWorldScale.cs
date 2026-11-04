using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad]
public static class CIM204PuzzleWorldScale
{
    static CIM204PuzzleWorldScale()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204PuzzleWorldScale.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var puzzle=Object.FindAnyObjectByType<FillGridPuzzle>();
        if(map==null||puzzle==null)return;
        var sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().ToDictionary(s=>s.name);
        var square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        if(square==null||!sprites.ContainsKey("Brick"))return;
        foreach(var collider in map.GetComponentsInChildren<BoxCollider2D>())
            if(collider.name=="Wall 35,17"||collider.name=="Wall 42,17"||collider.name=="Wall 36,21")
                Undo.DestroyObjectImmediate(collider.gameObject);
        var material=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Maps/Dungeon Wall.physicsMaterial2D");
        Vector2[] centers={new Vector2(35,20),new Vector2(42,20),new Vector2(38.5f,23),new Vector2(38.5f,21.5f)};
        Vector2[] sizes={new Vector2(1,7),new Vector2(1,7),new Vector2(6,1),new Vector2(6,2)};
        for(int i=0;i<4;i++){
            var obj=new GameObject(i==3?"Puzzle Chamber Floor Extension":"Puzzle Chamber Wall "+i);
            obj.transform.SetParent(map.transform,false);obj.transform.position=map.Cell(centers[i].x,centers[i].y);
            obj.transform.localScale=Vector3.one*map.cellSize;
            var r=obj.AddComponent<SpriteRenderer>();r.sprite=sprites[i==3?"Floor":"Brick"];r.drawMode=SpriteDrawMode.Tiled;
            r.size=sizes[i];r.sortingOrder=i==3?-10:-5;
            if(i!=3){var c=obj.AddComponent<BoxCollider2D>();c.size=sizes[i];c.sharedMaterial=material;}
        }
        puzzle.transform.position=map.Cell(38.5f,19.5f);
        puzzle.transform.localScale=new Vector3(9,9,1);
        puzzle.Build(square);EditorUtility.SetDirty(puzzle);
        var importer=AssetImporter.GetAtPath("Assets/Sprites/Kurenai spritesheet.png") as TextureImporter;
        if(importer!=null){importer.spritePixelsPerUnit=20;importer.SaveAndReimport();}
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))File.WriteAllText(marker,"Puzzle matches 1.5-unit map grid; character fits one tile.");
    }
}

