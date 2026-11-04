using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204SlugEnemies
{
    static CIM204SlugEnemies()=>EditorApplication.delayCall+=Apply;
    static void Configure(GameObject obj,Sprite[] frames)
    {
        var animation=obj.GetComponent<SlugAnimation>();
        if(animation==null) animation=obj.AddComponent<SlugAnimation>();
        animation.Configure(frames);
        var collider=obj.GetComponent<PolygonCollider2D>();
        if(collider!=null){
            collider.pathCount=1;
            collider.SetPath(0,new[]{new Vector2(-.48f,-.53f),new Vector2(-.48f,-.22f),
                new Vector2(-.2f,.1f),new Vector2(.45f,.1f),new Vector2(.45f,-.53f)});
            EditorUtility.SetDirty(collider);
        }
        obj.name=obj.name.Replace("Triangle Enemy","Slug Enemy");
        EditorUtility.SetDirty(obj);
        EditorUtility.SetDirty(animation);
        EditorUtility.SetDirty(obj.GetComponent<SpriteRenderer>());
    }
    static void Apply()
    {
        const string marker="Library/CIM204SlugEnemies.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        const string path="Assets/Sprites/Enemy Sprites.png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(importer==null||map==null)return;
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=24;importer.filterMode=FilterMode.Point;
        importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var rects=Enumerable.Range(0,8).Select(i=>new SpriteRect{name="Slug_"+i.ToString("D2"),
            rect=new Rect(i%4*32,32-(i/4)*32,32,32),pivot=new Vector2(.5f,.5f),
            alignment=SpriteAlignment.Center,spriteID=GUID.Generate()}).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
        if(frames.Length!=8)return;
        const string prefabPath="Assets/Prefabs/Triangle Enemy.prefab";
        var prefab=PrefabUtility.LoadPrefabContents(prefabPath);
        try{Configure(prefab,frames);PrefabUtility.SaveAsPrefabAsset(prefab,prefabPath);}
        finally{PrefabUtility.UnloadPrefabContents(prefab);}
        foreach(var enemy in Object.FindObjectsByType<TriangleEnemy>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(enemy.gameObject.scene!=map.gameObject.scene)continue;
            Configure(enemy.gameObject,frames);
            if(PrefabUtility.IsPartOfPrefabInstance(enemy)){
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.GetComponent<SpriteRenderer>());
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.GetComponent<PolygonCollider2D>());
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.GetComponent<SlugAnimation>());
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.gameObject);
            }
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))File.WriteAllText(marker,"Enemy prefab and all scene enemies now use the eight slug frames.");
    }
}
