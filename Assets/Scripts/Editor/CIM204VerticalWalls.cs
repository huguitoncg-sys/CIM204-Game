using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204VerticalWalls
{
    static CIM204VerticalWalls() => EditorApplication.delayCall += Apply;
    static bool Floor(ReferenceDungeonMap.Layout data,int x,int y) =>
        x>=0 && y>=0 && x<data.width && y<data.height && data.rows[y][x]=='.';
    static int Sides(ReferenceDungeonMap.Layout data,int x,int y)
    {
        if(x<0 || y<0 || x>=data.width || y>=data.height || data.rows[y][x]!='#') return 0;
        return (Floor(data,x-1,y)?1:0) | (Floor(data,x+1,y)?2:0);
    }
    static void Add(Transform root,string name,Sprite sprite,Vector3 position,Vector2 size,float scale,int order,Color color,bool flip=false)
    {
        var obj=new GameObject(name);obj.transform.SetParent(root,false);obj.transform.position=position;
        obj.transform.localScale=Vector3.one*scale;
        var renderer=obj.AddComponent<SpriteRenderer>();renderer.sprite=sprite;
        renderer.drawMode=SpriteDrawMode.Tiled;renderer.tileMode=SpriteTileMode.Continuous;
        renderer.size=size;renderer.sortingOrder=order;renderer.color=color;renderer.flipX=flip;
    }
    static void Apply()
    {
        const string marker="Library/CIM204VerticalWalls.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(map==null) return;
        const string path="Assets/Resources/CastleTiles.png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(importer==null) return;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var rects=provider.GetSpriteRects().ToList();
        if(!rects.Any(s=>s.name=="VerticalStone"))
            rects.Add(new SpriteRect{name="VerticalStone",rect=new Rect(224,512-184-16,4,16),
                alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),spriteID=GUID.Generate()});
        provider.SetSpriteRects(rects.ToArray());
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
        var data=JsonUtility.FromJson<ReferenceDungeonMap.Layout>(File.ReadAllText(AssetDatabase.GetAssetPath(map.layoutAsset)));
        var root=new GameObject("Castle Vertical Wall Edges").transform;root.SetParent(map.transform,false);
        int count=0;
        // Preserve full brick corners at horizontal junctions. Narrow stone edges meet those corners flush.
        for(int x=0;x<data.width;x++) for(int y=0;y<data.height;y++)
        {
            int sides=Sides(data,x,y);
            if(sides==0) continue;
            bool junction=Floor(data,x,y-1)||Floor(data,x,y+1);
            Vector3 p=map.Cell(x,y);
            if(!junction)
                Add(root,"Vertical Wall Core",sprites["Brick"],p,Vector2.one,map.cellSize,-4,Color.white);
            if((sides&1)!=0)
                Add(root,"Left Stone Edge",sprites["VerticalStone"],p+Vector3.left*(map.cellSize*.375f),
                    new Vector2(.25f,1),map.cellSize,-3,Color.white);
            if((sides&2)!=0)
                Add(root,"Right Stone Edge",sprites["VerticalStone"],p+Vector3.right*(map.cellSize*.375f),
                    new Vector2(.25f,1),map.cellSize,-3,Color.white,true);
            count++;
        }
        if(count==0){Object.DestroyImmediate(root.gameObject);Debug.LogError("No vertical walls found.");return;}
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))
            File.WriteAllText(marker,"Applied matching stone edges to "+count+" vertical wall cells; original colliders preserved.");
    }
}

