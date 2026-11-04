using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204BombSetup
{
    static CIM204BombSetup() => EditorApplication.delayCall += Apply;
    private static void Apply()
    {
        const string marker="Library/CIM204BombSetup.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var player=Object.FindAnyObjectByType<PlayerMovement>();
        if(map==null || player==null || Object.FindAnyObjectByType<BombRoomReward>()!=null) return;
        var scene=map.gameObject.scene;
        var targets=new List<TriangleEnemy>();
        Rect circleRoom=new Rect(11.5f,47.5f,18f,17f);
        foreach(var enemy in map.GetComponentsInChildren<TriangleEnemy>())
            if(circleRoom.Contains(map.Grid(enemy.transform.position))) targets.Add(enemy);
        if(targets.Count==0) throw new System.Exception("No active enemies found in southwest circle room.");
        var bombs=player.GetComponent<PlayerBombs>();
        if(bombs==null) bombs=Undo.AddComponent<PlayerBombs>(player.gameObject);
        bombs.circleSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CircleCollectible.png");
        EditorUtility.SetDirty(bombs);
        var reward=new GameObject("Southwest Circle - Bomb Reward"); reward.transform.SetParent(map.transform);
        var encounter=reward.AddComponent<BombRoomReward>(); encounter.map=map; encounter.enemies=targets.ToArray();
        var wall=new GameObject("Red Breakable Wall - Maze Puzzle Entrance"); wall.transform.SetParent(map.transform);
        wall.transform.position=map.Cell(38.5f,16);
        wall.transform.localScale=new Vector3(map.cellSize*2,map.cellSize,1);
        var renderer=wall.AddComponent<SpriteRenderer>();
        renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        renderer.color=new Color(0.95f,0.12f,0.12f); renderer.sortingOrder=5;
        wall.AddComponent<BoxCollider2D>(); wall.AddComponent<BombBreakableWall>();
        Undo.RegisterCreatedObjectUndo(wall,"Add bomb breakable wall");
        Undo.RegisterCreatedObjectUndo(reward,"Add bomb room reward");
        EditorSceneManager.MarkSceneDirty(scene);
        if(EditorSceneManager.SaveScene(scene)) File.WriteAllText(marker,"Bomb unlock tracks "+targets.Count+" southwest enemies; red maze wall installed.");
    }
}
