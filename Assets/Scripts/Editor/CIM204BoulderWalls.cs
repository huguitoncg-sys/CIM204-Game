using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204BoulderWalls
{
    static CIM204BoulderWalls() => EditorApplication.delayCall += Apply;
    private static void Apply()
    {
        const string marker = "Library/CIM204BoulderConnectedWalls.complete";
        if (File.Exists(marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var puzzle = Object.FindAnyObjectByType<BoulderPuzzleRoom>();
        if (puzzle == null) return;
        Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        foreach (Transform child in puzzle.transform)
            if (child.name == "Puzzle Border") { Undo.RecordObject(child.gameObject,"Replace puzzle border"); child.gameObject.SetActive(false); }
        float left = puzzle.map.Cell(17.5f,16).x;
        float right = puzzle.map.Cell(23.5f,16).x;
        foreach (int index in new int[] {2,32})
        {
            Vector3 center = puzzle.CellPosition(index);
            float openingLeft = center.x - puzzle.tileSize/2;
            float openingRight = center.x + puzzle.tileSize/2;
            CreateWall(puzzle,square,left,openingLeft,center.y,index+" Left");
            CreateWall(puzzle,square,openingRight,right,center.y,index+" Right");
        }
        EditorSceneManager.MarkSceneDirty(puzzle.gameObject.scene);
        if (EditorSceneManager.SaveScene(puzzle.gameObject.scene)) File.WriteAllText(marker,"Connected top and bottom puzzle walls.");
    }
    private static void CreateWall(BoulderPuzzleRoom puzzle, Sprite sprite, float left, float right, float y, string name)
    {
        GameObject wall = new GameObject("Connected Puzzle Wall " + name);
        Undo.RegisterCreatedObjectUndo(wall,"Connect puzzle walls");
        wall.transform.SetParent(puzzle.transform);
        wall.transform.position = new Vector3((left+right)/2,y,0);
        wall.transform.localScale = new Vector3(right-left,puzzle.tileSize,1);
        var renderer = wall.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
        renderer.color = new Color(0.035f,0.09f,0.055f); renderer.sortingOrder = -1;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Maps/Dungeon Wall.physicsMaterial2D");
    }
}
