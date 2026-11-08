using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204MazeProgression
{
    static CIM204MazeProgression()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204MazeProgression.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var puzzle=Object.FindAnyObjectByType<FillGridPuzzle>();
        var brick=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Brick");
        if(map==null||puzzle==null||brick==null)return;
        var old=map.transform.Find("Tile Puzzle - Locked Passages");
        if(old!=null)Undo.DestroyObjectImmediate(old.gameObject);
        var gateRoot=new GameObject("Maze Completion Gate");gateRoot.transform.SetParent(map.transform,false);
        gateRoot.AddComponent<TilePuzzleBarriers>().Configure(puzzle);
        Create(map,gateRoot.transform,brick,new Vector2(50.5f,21),new Vector2(2,1),"Maze Exit - Complete Tile Puzzle");
        Create(map,map.transform,brick,new Vector2(24,27.5f),new Vector2(1,2),"Permanent Wall - Room 2 to Room 9");
        Create(map,map.transform,brick,new Vector2(24,30.5f),new Vector2(1,2),"Permanent Wall - Room 3 to Room 9");
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))File.WriteAllText(marker,"Maze exit is puzzle-gated; west shortcuts permanently closed.");
    }
    static void Create(ReferenceDungeonMap map,Transform parent,Sprite brick,Vector2 cell,Vector2 size,string name)
    {
        var wall=new GameObject(name);wall.transform.SetParent(parent,false);
        wall.transform.position=map.Cell(cell.x,cell.y);wall.transform.localScale=Vector3.one*map.cellSize;
        var renderer=wall.AddComponent<SpriteRenderer>();renderer.sprite=brick;renderer.color=Color.white;
        renderer.drawMode=SpriteDrawMode.Tiled;renderer.tileMode=SpriteTileMode.Continuous;
        renderer.size=size;renderer.sortingOrder=5;
        var collider=wall.AddComponent<BoxCollider2D>();collider.size=size;
        collider.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Maps/Dungeon Wall.physicsMaterial2D");
    }
}
