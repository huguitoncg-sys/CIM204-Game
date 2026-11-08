using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204HealingStation
{
    static CIM204HealingStation()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204HealingStationRoom14.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        if(map==null||square==null)return;
        var station=new GameObject("healing station");
        var renderer=station.AddComponent<SpriteRenderer>();
        renderer.sprite=square;renderer.color=new Color(1f,.12f,.16f);renderer.sortingOrder=15;
        station.AddComponent<BoxCollider2D>().isTrigger=true;
        station.AddComponent<HealingStation>();
        var prefab=PrefabUtility.SaveAsPrefabAsset(station,"Assets/Prefabs/Healing Station.prefab");
        Object.DestroyImmediate(station);
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,map.gameObject.scene);
        instance.transform.SetParent(map.transform,false);
        instance.transform.position=map.Cell(55.5f,51.5f);
        instance.transform.localScale=Vector3.one*1.2f;
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))File.WriteAllText(marker,"Reusable full-health station in room 14.");
    }
}
