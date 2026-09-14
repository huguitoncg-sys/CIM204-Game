using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CIM204RoomSetup
{
    private const string Marker = "Library/CIM204RoomSetup.complete";
    static CIM204RoomSetup() => EditorApplication.delayCall += Setup;

    [MenuItem("CIM204/Connect Background Rooms")]
    public static void Setup()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Setup;
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        GameObject first = null, second = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "Background Rectangle") first = child.gameObject;
                if (child.name == "Background Rectangle (1)") second = child.gameObject;
            }
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        Camera camera = Camera.main;
        if (first == null || second == null || player == null || camera == null) return;
        if (first.GetComponent<SpriteRenderer>() == null || second.GetComponent<SpriteRenderer>() == null) return;
        RoomArea left = first.GetComponent<RoomArea>();
        if (left == null) left = Undo.AddComponent<RoomArea>(first);
        RoomArea right = second.GetComponent<RoomArea>();
        if (right == null) right = Undo.AddComponent<RoomArea>(second);
        if (left.Bounds.center.x > right.Bounds.center.x) { RoomArea swap = left; left = right; right = swap; }
        Undo.RecordObject(left, "Connect rooms");
        Undo.RecordObject(right, "Connect rooms");
        left.rightRoom = right;
        right.leftRoom = left;
        PlayerRoomTransition transition = player.GetComponent<PlayerRoomTransition>();
        if (transition == null) transition = Undo.AddComponent<PlayerRoomTransition>(player.gameObject);
        Undo.RecordObject(transition, "Configure room transition");
        transition.Configure(left, camera);
        EditorUtility.SetDirty(left);
        EditorUtility.SetDirty(right);
        EditorUtility.SetDirty(transition);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new System.Exception("Could not save room transitions.");
        File.WriteAllText(Marker, "Connected both rooms and saved scene.");
        Debug.Log("CIM204: Room transitions ready. Walk to the turquoise room's right edge; return using the blue room's left edge.");
    }
}
