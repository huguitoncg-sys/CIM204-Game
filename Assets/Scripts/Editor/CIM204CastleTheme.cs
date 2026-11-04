using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204CastleTheme
{
    const string Atlas = "Assets/Resources/CastleTiles.png";
    const string Marker = "Library/CIM204CastleTheme.complete";
    static CIM204CastleTheme() => EditorApplication.delayCall += Apply;
    static Rect Top(int x, int y, int w=16, int h=16) => new Rect(x,512-y-h,w,h);
    [MenuItem("CIM204/Apply Castle Theme")]
    static void Apply()
    {
        if (File.Exists(Marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Apply; return; }
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var importer=AssetImporter.GetAtPath(Atlas) as TextureImporter;
        if(map==null || importer==null) return;
        string backup=AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Before Castle Theme.unity");
        if(!EditorSceneManager.SaveScene(map.gameObject.scene,backup,true)) return;
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit=16;
        importer.filterMode=FilterMode.Point;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled=false; importer.alphaIsTransparency=true;
        var specs=new Dictionary<string,Rect> {
            {"Floor",Top(0,0)}, {"FloorWorn",Top(176,32)}, {"Brick",Top(16,64)},
            {"ChestClosed",Top(16,144)}, {"ChestOpen",Top(48,160)},
            {"Crate",Top(96,364,16,20)}, {"Torch",Top(32,24,16,24)}, {"Banner",Top(32,80)},
            {"Gate",Top(80,144,32,32)}
        };
        var factory=new SpriteDataProviderFactories(); factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var slices=specs.Select(s=>new SpriteRect {name=s.Key,rect=s.Value,pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()}).ToArray();
        provider.SetSpriteRects(slices);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(slices.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply(); importer.SaveAndReimport();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(Atlas).OfType<Sprite>().ToDictionary(s=>s.name);
        if(!AssetDatabase.IsValidFolder("Assets/Maps/Castle")) AssetDatabase.CreateFolder("Assets/Maps","Castle");
        var material=new Material(Shader.Find("Sprites/Default"));
        material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas);
        AssetDatabase.CreateAsset(material,"Assets/Maps/Castle/Atlas.mat");
        var root=new GameObject("Castle Environment"); root.transform.SetParent(map.transform,false);
        BuildSurface(map,root,material,specs,false);
        BuildSurface(map,root,material,specs,true);
        foreach(var renderer in map.GetComponentsInChildren<SpriteRenderer>())
        {
            if(renderer.name.StartsWith("Floor ") || renderer.name.StartsWith("Wall "))
                renderer.enabled=false; // Original wall colliders stay active.
            if(renderer.GetComponent<FillGridPuzzle>()!=null) renderer.color=new Color(.28f,.30f,.40f);
            if(renderer.name.StartsWith("Connected Puzzle Wall") || renderer.name=="Puzzle Border")
            { renderer.sprite=sprites["Brick"]; renderer.color=Color.white; }
            if(renderer.name.StartsWith("Puzzle Floor") && renderer.color.g>renderer.color.r)
            { renderer.sprite=sprites["Floor"]; renderer.color=new Color(.85f,.95f,1f); }
            if(renderer.GetComponent<BombBreakableWall>()!=null)
            { renderer.sprite=sprites["Brick"]; renderer.color=new Color(1f,.32f,.3f); }
        }
        foreach(var puzzle in map.GetComponentsInChildren<BoulderPuzzleRoom>())
            foreach(var rock in puzzle.boulders)
                if(rock!=null) {var r=rock.GetComponent<SpriteRenderer>();r.sprite=sprites["Crate"];r.color=Color.white;EditorUtility.SetDirty(r);}
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Boulder.prefab");
        if(prefab!=null) {prefab.GetComponent<SpriteRenderer>().sprite=sprites["Crate"];EditorUtility.SetDirty(prefab);}
        var encounter=Object.FindAnyObjectByType<FirstRoomEncounter>();
        if(encounter!=null) foreach(var door in encounter.yellowDoors)
            if(door!=null) {
                var r=door.GetComponent<SpriteRenderer>();
                // A 32px door is two units wide; tile into the existing doorway footprint.
                r.sprite=sprites["Gate"];r.drawMode=SpriteDrawMode.Sliced;r.size=Vector2.one;
                r.color=new Color(1f,.85f,.45f);
            }
        // Sparse wall decorations, never in walkable cells.
        for(int y=1;y<map.Data.height-1;y++) for(int x=1;x<map.Data.width-1;x++)
            if(map.Data.rows[y][x]=='#' && map.Data.rows[y+1][x]=='.' &&
               map.Data.rows[y][x-1]=='#' && map.Data.rows[y][x+1]=='#' && x%6==0)
            {
                var decoration=new GameObject("Wall Torch");decoration.transform.SetParent(root.transform,false);
                decoration.transform.position=map.Cell(x,y);
                decoration.transform.localScale=Vector3.one*.75f;
                var r=decoration.AddComponent<SpriteRenderer>();r.sprite=sprites["Torch"];r.sortingOrder=-3;
            }
        if(Camera.main!=null) {Camera.main.backgroundColor=new Color(.055f,.06f,.09f);EditorUtility.SetDirty(Camera.main);}
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene)) File.WriteAllText(Marker,"Castle art applied. Backup: "+backup);
    }
    static void BuildSurface(ReferenceDungeonMap map,GameObject root,Material material,Dictionary<string,Rect> specs,bool walls)
    {
        var vertices=new List<Vector3>();var uvs=new List<Vector2>();var colors=new List<Color>();var indices=new List<int>();
        float half=map.cellSize*.5f;
        for(int y=0;y<map.Data.height;y++) for(int x=0;x<map.Data.width;x++)
        {
            if(map.Data.rows[y][x]!=(walls?'#':'.')) continue;
            Vector3 p=map.Cell(x,y)-map.transform.position;int i=vertices.Count;
            vertices.Add(p+new Vector3(-half,-half));vertices.Add(p+new Vector3(-half,half));
            vertices.Add(p+new Vector3(half,half));vertices.Add(p+new Vector3(half,-half));
            Rect r=specs[walls?"Brick":((x*17+y*31)%19==0?"FloorWorn":"Floor")];
            float x0=(r.xMin+.05f)/512f,x1=(r.xMax-.05f)/512f,y0=(r.yMin+.05f)/512f,y1=(r.yMax-.05f)/512f;
            uvs.Add(new Vector2(x0,y0));uvs.Add(new Vector2(x0,y1));uvs.Add(new Vector2(x1,y1));uvs.Add(new Vector2(x1,y0));
            bool edge=!walls && (x==0||y==0||x==map.Data.width-1||y==map.Data.height-1||
                map.Data.rows[y-1][x]!='.'||map.Data.rows[y+1][x]!='.'||map.Data.rows[y][x-1]!='.'||map.Data.rows[y][x+1]!='.');
            Color tint=walls?new Color(.9f,.92f,1f):edge?new Color(.66f,.72f,.78f):new Color(.86f,.91f,.97f);
            for(int n=0;n<4;n++)colors.Add(tint);
            indices.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});
        }
        var mesh=new Mesh {name=walls?"Castle Walls":"Castle Floor",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
        mesh.SetVertices(vertices);mesh.SetUVs(0,uvs);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,"Assets/Maps/Castle/"+mesh.name+".asset");
        var obj=new GameObject(mesh.name);obj.transform.SetParent(root.transform,false);
        obj.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=walls?-5:-10;
    }
}

