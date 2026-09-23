using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CIM204MeleeSetup
{
    static CIM204MeleeSetup() => EditorApplication.delayCall += Setup;
    [MenuItem("CIM204/Add Player Melee")]
    public static void Setup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Setup;
            return;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        if (player == null || player.GetComponent<PlayerMelee>() != null) return;
        Undo.AddComponent<PlayerMelee>(player.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new System.Exception("Could not save melee setup.");
        File.WriteAllText("Library/CIM204MeleeSetup.complete", "Player melee added and scene saved.");
        Debug.Log("CIM204: Space attacks a facing half-circle; triangles die after three hits.");
    }
}
