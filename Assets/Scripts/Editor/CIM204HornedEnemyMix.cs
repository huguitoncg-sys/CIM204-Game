using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204HornedEnemyMix
{
    static CIM204HornedEnemyMix()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204HornedEnemyMix.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        const string path="Assets/Sprites/Enemy Sprites.png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(map==null||importer==null)return;
        var enemies=Object.FindObjectsByType<TriangleEnemy>(FindObjectsSortMode.None)
            .Where(e=>e.gameObject.scene==map.gameObject.scene && e.gameObject.activeInHierarchy).ToArray();
        if(enemies.Length<2)return;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var rects=provider.GetSpriteRects().ToList();
        for(int i=0;i<8;i++){
            string name="Horned_"+i.ToString("D2");
            if(rects.Any(s=>s.name==name))continue;
            rects.Add(new SpriteRect{name=name,rect=new Rect(i%4*32,160-(i/4+2)*32,32,32),
                pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()});
        }
        provider.SetSpriteRects(rects.ToArray());
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Where(s=>s.name.StartsWith("Horned_")).OrderBy(s=>s.name).ToArray();
        if(frames.Length!=8)return;
        var random=new System.Random();
        for(int i=enemies.Length-1;i>0;i--){int j=random.Next(i+1);var temp=enemies[i];enemies[i]=enemies[j];enemies[j]=temp;}
        int count=enemies.Length/2;
        for(int i=0;i<count;i++){
            var enemy=enemies[i];
            var animation=enemy.GetComponent<SlugAnimation>();
            if(animation==null)animation=enemy.gameObject.AddComponent<SlugAnimation>();
            animation.Configure(frames);
            EditorUtility.SetDirty(animation);EditorUtility.SetDirty(enemy.GetComponent<SpriteRenderer>());
            if(PrefabUtility.IsPartOfPrefabInstance(enemy)){
                PrefabUtility.RecordPrefabInstancePropertyModifications(animation);
                PrefabUtility.RecordPrefabInstancePropertyModifications(enemy.GetComponent<SpriteRenderer>());
            }
        }
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))
            File.WriteAllText(marker,"Randomly assigned horned animation to "+count+" of "+enemies.Length+" active enemies; gameplay unchanged.");
    }
}
