using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class CIM204SmallerDetection
{
    static CIM204SmallerDetection() => EditorApplication.delayCall += Apply;
    private static void Apply()
    {
        const string marker = "Library/CIM204SmallerDetection.complete";
        if (File.Exists(marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Apply;
            return;
        }
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Triangle Enemy.prefab");
        if (prefab != null)
        {
            foreach (var enemy in prefab.GetComponentsInChildren<TriangleEnemy>(true)) SetRadius(enemy);
            PrefabUtility.SavePrefabAsset(prefab);
        }
        foreach (var root in scene.GetRootGameObjects())
            foreach (var enemy in root.GetComponentsInChildren<TriangleEnemy>(true)) SetRadius(enemy);
        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene)) File.WriteAllText(marker, "Detection radius set to 2.");
    }
    private static void SetRadius(TriangleEnemy enemy)
    {
        var serialized = new SerializedObject(enemy);
        serialized.FindProperty("detectionRadius").floatValue = 2f;
        serialized.ApplyModifiedProperties();
    }
}
