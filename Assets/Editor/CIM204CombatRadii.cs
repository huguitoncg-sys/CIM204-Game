using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class CIM204CombatRadii
{
    static CIM204CombatRadii() => EditorApplication.delayCall += Apply;
    private static void Apply()
    {
        const string marker = "Library/CIM204CombatRadii3And19.complete";
        if (File.Exists(marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Apply;
            return;
        }
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        // Supersede the earlier one-time radius adjustment if it is still pending.
        File.WriteAllText("Library/CIM204SmallerDetection.complete", "Superseded by radius 3.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Triangle Enemy.prefab");
        if (prefab != null)
        {
            foreach (var enemy in prefab.GetComponentsInChildren<TriangleEnemy>(true)) SetRadius(enemy, "detectionRadius", 3f);
            PrefabUtility.SavePrefabAsset(prefab);
        }
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var enemy in root.GetComponentsInChildren<TriangleEnemy>(true)) SetRadius(enemy, "detectionRadius", 3f);
            foreach (var melee in root.GetComponentsInChildren<PlayerMelee>(true)) SetRadius(melee, "attackRadius", 1.9f);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (EditorSceneManager.SaveScene(scene)) File.WriteAllText(marker, "Enemy radius 3; melee radius 1.9.");
    }
    private static void SetRadius(Object component, string field, float value)
    {
        var serialized = new SerializedObject(component);
        serialized.FindProperty(field).floatValue = value;
        serialized.ApplyModifiedProperties();
    }
}
