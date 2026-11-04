using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204TilePuzzleBarriers
{
    static CIM204TilePuzzleBarriers() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204TilePuzzleBarriers.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var puzzle=Object.FindAnyObjectByType<FillGridPuzzle>();
        var brick=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Brick");
        if(map==null || puzzle==null || brick==null) return;
        var root=new GameObject("Tile Puzzle - Locked Passages");
        root.transform.SetParent(map.transform,false);
        root.AddComponent<TilePuzzleBarriers>().Configure(puzzle);
        Vector2[] centers={new Vector2(24,27.5f),new Vector2(24,30.5f),new Vector2(50.5f,16)};
        Vector2[] sizes={new Vector2(1,2),new Vector2(1,2),new Vector2(2,1)};
        var material=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Maps/Dungeon Wall.physicsMaterial2D");
        for(int i=0;i<centers.Length;i++)
        {
            var wall=new GameObject(i==2?"Maze South Puzzle Gate":"West Hall Puzzle Gate "+(i+1));
            wall.transform.SetParent(root.transform,false);
            wall.transform.position=map.Cell(centers[i].x,centers[i].y);
            wall.transform.localScale=Vector3.one*map.cellSize;
            var renderer=wall.AddComponent<SpriteRenderer>();
            renderer.sprite=brick;renderer.color=Color.white;
            renderer.drawMode=SpriteDrawMode.Tiled;renderer.tileMode=SpriteTileMode.Continuous;
            renderer.size=sizes[i];renderer.sortingOrder=5;
            var collider=wall.AddComponent<BoxCollider2D>();collider.size=sizes[i];collider.sharedMaterial=material;
        }
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))File.WriteAllText(marker,"Three passage walls unlock when all tile puzzle cells are filled.");
    }
}
