using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204DaimaSetup
{
    static CIM204DaimaSetup()=>EditorApplication.delayCall+=Apply;
    static Sprite[] Slice(string path,int columns,int rows,int cell,float ppu,string prefix)
    {
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(importer==null)return null;
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var old=provider.GetSpriteRects();
        var slices=Enumerable.Range(0,columns*rows).Select(i=>{
            string name=prefix+i.ToString("D2");var existing=old.FirstOrDefault(s=>s.name==name);
            return new SpriteRect{name=name,rect=new Rect(i%columns*cell,(rows-1-i/columns)*cell,cell,cell),
                alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),spriteID=existing!=null?existing.spriteID:GUID.Generate()};
        }).ToArray();
        provider.SetSpriteRects(slices);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(slices.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    }
    static void Apply()
    {
        const string marker="Library/CIM204DaimaSetup.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();if(map==null)return;
        var frames=Slice("Assets/Sprites/DaimaSpritesheet.png",3,4,128,32,"Daima_");
        var ghosts=Slice("Assets/Sprites/CIM204_Ghost.png",2,2,32,24,"Ghost_");
        if(frames==null||ghosts==null)return;
        var obj=new GameObject("Daima - Final Boss");obj.transform.SetParent(map.transform,false);obj.transform.position=map.Cell(68.5f,78);
        obj.transform.localScale=new Vector3(2,2,1);
        var r=obj.AddComponent<SpriteRenderer>();r.sprite=frames[0];r.sortingOrder=20;
        var c=obj.AddComponent<CircleCollider2D>();c.isTrigger=true;c.radius=1.2f;
        var body=obj.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;
        var boss=obj.AddComponent<DaimaBoss>();boss.map=map;boss.frames=frames;boss.ghostFrames=ghosts;
        boss.square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        boss.circle=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CircleCollectible.png");
        var player=Object.FindAnyObjectByType<PlayerMovement>();
        if(player!=null && player.GetComponent<PlayerStun>()==null)player.gameObject.AddComponent<PlayerStun>();
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))File.WriteAllText(marker,"Daima placed in largest room: 25 HP and five attacks.");
    }
}

