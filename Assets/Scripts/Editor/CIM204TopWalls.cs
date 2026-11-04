using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204TopWalls
{
    static CIM204TopWalls() => EditorApplication.delayCall += Apply;
    [MenuItem("CIM204/Apply Castle Top Walls")]
    static void Apply()
    {
        const string marker="Library/CIM204TopWalls.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        const string path="Assets/Resources/CastleTiles.png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(map==null || importer==null) return;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var rects=provider.GetSpriteRects().ToList();
        if(!rects.Any(s=>s.name=="TopWall"))
            rects.Add(new SpriteRect {name="TopWall",rect=new Rect(16,512-270-16,48,16),
                pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()});
        provider.SetSpriteRects(rects.ToArray());
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First(s=>s.name=="TopWall");
        var data=JsonUtility.FromJson<ReferenceDungeonMap.Layout>(File.ReadAllText(AssetDatabase.GetAssetPath(map.layoutAsset)));
        var root=map.transform.Find("Castle Top Walls");
        if(root==null){root=new GameObject("Castle Top Walls").transform;root.SetParent(map.transform,false);}
        int count=0;
        for(int y=0;y<data.height-1;y++)
        for(int x=0;x<data.width;x++)
        {
            if(data.rows[y][x]!='#' || data.rows[y+1][x]!='.') continue;
            int start=x;
            while(x+1<data.width && data.rows[y][x+1]=='#' && data.rows[y+1][x+1]=='.') x++;
            var wall=new GameObject("Top Wall "+start+","+y);
            wall.transform.SetParent(root,false);
            wall.transform.position=map.Cell((start+x)*.5f,y);
            // Match the 16-pixel tile scale and one-cell height of the other walls.
            float scale=map.cellSize;
            wall.transform.localScale=Vector3.one*scale;
            var renderer=wall.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite;renderer.drawMode=SpriteDrawMode.Tiled;
            renderer.size=new Vector2((x-start+1)*map.cellSize/scale,1f);
            renderer.sortingOrder=-4;
            count++;
        }
        if(count==0){Debug.LogError("No top walls found; layout has not been modified.");return;}
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene))File.WriteAllText(marker,"Updated "+count+" top wall runs.");
    }
}

