using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204FirstEncounterSetup
{
    static CIM204FirstEncounterSetup() => EditorApplication.delayCall += Apply;
    private static void Apply()
    {
        const string marker="Library/CIM204FirstEncounter.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(map==null || Object.FindAnyObjectByType<FirstRoomEncounter>()!=null) return;
        var scene=map.gameObject.scene;
        var room=new Rect(12.5f,4.5f,14f,8f);
        TriangleEnemy enemy=null;
        foreach(var candidate in map.GetComponentsInChildren<TriangleEnemy>())
            if(room.Contains(map.Grid(candidate.transform.position))) { enemy=candidate; break; }
        if(enemy==null)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Triangle Enemy.prefab");
            if(prefab==null) throw new System.Exception("Triangle enemy prefab missing.");
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            instance.transform.SetParent(map.transform); enemy=instance.GetComponent<TriangleEnemy>();
        }
        Undo.RecordObject(enemy.transform,"Center tutorial enemy");
        enemy.transform.position=map.Cell(19.5f,8.5f);
        if(PrefabUtility.IsPartOfPrefabInstance(enemy)) PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.transform);
        var root=new GameObject("First Room - Defeat Enemy"); root.transform.SetParent(map.transform);
        Undo.RegisterCreatedObjectUndo(root,"Create first room encounter");
        var encounter=root.AddComponent<FirstRoomEncounter>(); encounter.map=map; encounter.enemy=enemy;
        encounter.yellowDoors=new GameObject[2];
        Sprite square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        Vector2[] gates={new Vector2(27,9.5f),new Vector2(20.5f,13)};
        for(int i=0;i<gates.Length;i++)
        {
            var door=new GameObject(i==0 ? "Yellow Door - Maze Exit" : "Yellow Door - South Exit");
            door.transform.SetParent(root.transform); door.transform.position=map.Cell(gates[i].x,gates[i].y);
            door.transform.localScale=Vector3.one*(map.cellSize*2);
            var renderer=door.AddComponent<SpriteRenderer>(); renderer.sprite=square;
            renderer.color=new Color(1,0.85f,0.05f); renderer.sortingOrder=5;
            var collider=door.AddComponent<BoxCollider2D>();
            collider.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Maps/Dungeon Wall.physicsMaterial2D");
            encounter.yellowDoors[i]=door;
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if(EditorSceneManager.SaveScene(scene)) File.WriteAllText(marker,"First room popup and yellow doors added.");
    }
}
