using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204CastleDoors
{
    static CIM204CastleDoors() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204CastleDoors.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var encounter=Object.FindAnyObjectByType<FirstRoomEncounter>();
        if(encounter==null || encounter.map==null) return;
        const string path="Assets/Resources/CastleTiles.png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(importer==null) return;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var rects=provider.GetSpriteRects().ToList();
        string[] names={"ArchedDoorClosed","ArchedDoorOpen"};
        Rect[] bounds={new Rect(76,512-140-36,40,36),new Rect(124,512-140-36,40,36)};
        for(int i=0;i<2;i++)
        {
            var rect=rects.FirstOrDefault(s=>s.name==names[i]);
            if(rect==null){rect=new SpriteRect{name=names[i],spriteID=GUID.Generate(),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)};rects.Add(rect);}
            rect.rect=bounds[i];
        }
        provider.SetSpriteRects(rects.ToArray());
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
        foreach(var door in encounter.yellowDoors)
        {
            if(door==null) continue;
            var renderer=door.GetComponent<SpriteRenderer>();
            var collider=door.GetComponent<BoxCollider2D>();
            Vector2 oldScale=door.transform.lossyScale;
            Vector2 worldCollider=collider!=null?Vector2.Scale(collider.size,oldScale):Vector2.one*3;
            Vector2 worldOffset=collider!=null?Vector2.Scale(collider.offset,oldScale):Vector2.zero;
            Vector3 parent=door.transform.parent.lossyScale;
            float scale=encounter.map.cellSize;
            door.transform.localScale=new Vector3(scale/parent.x,scale/parent.y,1);
            renderer.sprite=sprites["ArchedDoorClosed"];
            renderer.drawMode=SpriteDrawMode.Simple;
            renderer.color=Color.white;
            if(collider!=null){collider.size=worldCollider/scale;collider.offset=worldOffset/scale;EditorUtility.SetDirty(collider);}
            var behavior=door.GetComponent<CastleDoor>();
            if(behavior==null) behavior=door.AddComponent<CastleDoor>();
            behavior.Configure(sprites["ArchedDoorOpen"]);
            EditorUtility.SetDirty(behavior);EditorUtility.SetDirty(renderer);EditorUtility.SetDirty(door.transform);
        }
        EditorSceneManager.MarkSceneDirty(encounter.gameObject.scene);
        if(EditorSceneManager.SaveScene(encounter.gameObject.scene))File.WriteAllText(marker,"Arched doors at wall pixel scale; frame remains when opened.");
    }
}
