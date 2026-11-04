using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204BoulderSetup
{
    static CIM204BoulderSetup() => EditorApplication.delayCall += Setup;
    [MenuItem("CIM204/Add Boulder Puzzle Room")]
    public static void Setup()
    {
        const string marker = "Library/CIM204BoulderPuzzle.complete";
        if (File.Exists(marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Setup; return; }
        var map = Object.FindAnyObjectByType<ReferenceDungeonMap>();
        var player = Object.FindAnyObjectByType<PlayerMovement>();
        if (map == null || player == null || Object.FindAnyObjectByType<BoulderPuzzleRoom>() != null) return;
        var scene = map.gameObject.scene;
        string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Before Boulder Puzzle.unity");
        if (!EditorSceneManager.SaveScene(scene, backup, true)) throw new System.Exception("Could not back up scene.");
        const string spritePath = "Assets/Sprites/RoundedBoulder.png";
        var texture = new Texture2D(128,128,TextureFormat.RGBA32,false);
        var pixels = new Color[128*128];
        for(int y=0;y<128;y++) for(int x=0;x<128;x++)
        {
            Vector2 q = new Vector2(Mathf.Abs(x-63.5f)-43, Mathf.Abs(y-63.5f)-43);
            float distance = new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude + Mathf.Min(Mathf.Max(q.x,q.y),0)-18;
            float shade = Mathf.Lerp(0.63f,0.9f,y/127f);
            if (distance > -5) shade *= 0.7f;
            pixels[y*128+x] = new Color(shade,shade*0.86f,shade*0.68f,Mathf.Clamp01(-distance));
        }
        texture.SetPixels(pixels); texture.Apply();
        File.WriteAllBytes(spritePath, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(spritePath,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(spritePath);
        importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=128; importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        Sprite rockSprite=AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        Sprite square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        GameObject template=new GameObject("Boulder");
        var rockRenderer=template.AddComponent<SpriteRenderer>(); rockRenderer.sprite=rockSprite; rockRenderer.sortingOrder=15;
        template.AddComponent<BoxCollider2D>().size=new Vector2(0.92f,0.92f);
        string prefabPath=AssetDatabase.GenerateUniqueAssetPath("Assets/Prefabs/Boulder.prefab");
        GameObject prefab=PrefabUtility.SaveAsPrefabAsset(template,prefabPath); Object.DestroyImmediate(template);
        GameObject root=new GameObject("Boulder Puzzle - West Upper Room");
        root.transform.SetParent(map.transform,false); root.transform.position=map.Cell(20.5f,16);
        Undo.RegisterCreatedObjectUndo(root,"Create Boulder Puzzle");
        var puzzle=root.AddComponent<BoulderPuzzleRoom>(); puzzle.map=map;
        puzzle.boulders=new Transform[12];
        for(int i=0;i<12;i++)
        {
            var rock=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            rock.name="Boulder "+(i+1); rock.transform.SetParent(root.transform); puzzle.boulders[i]=rock.transform;
        }
        puzzle.ResetBoulders();
        for(int y=0;y<7;y++) for(int x=0;x<5;x++)
        {
            int index=y*5+x;
            bool floor=index==2 || index==32 || (y>0 && y<6);
            var tile=new GameObject(floor ? "Puzzle Floor "+x+","+y : "Puzzle Border");
            tile.transform.SetParent(root.transform); tile.transform.position=puzzle.CellPosition(index);
            tile.transform.localScale=Vector3.one*1.15f;
            var renderer=tile.AddComponent<SpriteRenderer>(); renderer.sprite=square; renderer.sortingOrder=-2;
            renderer.color=!floor ? new Color(0.06f,0.15f,0.14f) : index==2 ? new Color(0.55f,0.92f,0.85f) : index==32 ? new Color(1,0.8f,0.2f) : new Color(0.16f,0.55f,0.49f);
        }
        // Keep combat enemies out of the puzzle chamber, preserving them for later editing.
        Rect room=new Rect(17.5f,13.5f,7f,15f);
        foreach(var enemy in map.GetComponentsInChildren<TriangleEnemy>())
            if(room.Contains(map.Grid(enemy.transform.position))) { Undo.RecordObject(enemy.gameObject,"Clear puzzle chamber"); enemy.gameObject.SetActive(false); }
        if(player.GetComponent<PlayerInteraction>()==null) Undo.AddComponent<PlayerInteraction>(player.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        if(!EditorSceneManager.SaveScene(scene)) throw new System.Exception("Could not save boulder puzzle.");
        File.WriteAllText(marker,"Boulder puzzle created; backup: "+backup);
        Selection.activeGameObject=root;
        Debug.Log("CIM204: Boulder puzzle installed in upper west room. E pushes; leaving fades and resets boulders.");
    }
}
