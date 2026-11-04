using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204EntranceSpawn
{
    static CIM204EntranceSpawn() => EditorApplication.delayCall += Apply;
    private static void Apply()
    {
        const string marker = "Library/CIM204EntranceSpawnFixed.complete";
        if (File.Exists(marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var map = Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var player = Object.FindAnyObjectByType<PlayerMovement>();
        if (map == null || player == null) return;
        Vector3 position = map.Cell(7, 8.5f);
        var collider = player.GetComponent<BoxCollider2D>();
        // Use the known clear entrance tile; live collider bounds may be stale during editor reload.
        Undo.RecordObject(player.transform,"Move player spawn to entrance");
        position.z = player.transform.position.z;
        player.transform.position = position;
        var body = player.GetComponent<Rigidbody2D>();
        if (body != null) { Undo.RecordObject(body,"Move player spawn to entrance"); body.position = position; }
        if (PrefabUtility.IsPartOfPrefabInstance(player.gameObject)) PrefabUtility.RecordPrefabInstancePropertyModifications(player.transform);
        var camera = Camera.main;
        if (camera != null)
        {
            Undo.RecordObject(camera.transform,"Frame player spawn");
            camera.transform.position = new Vector3(position.x,position.y,camera.transform.position.z);
        }
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        if (EditorSceneManager.SaveScene(player.gameObject.scene)) File.WriteAllText(marker,"Player starts in upper-left entrance room.");
    }
}

