using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204WallScale
{
    static CIM204WallScale() => EditorApplication.delayCall += Apply;
    static void Apply()
    {
        const string marker="Library/CIM204WallScale.complete";
        if(File.Exists(marker)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var map=Object.FindAnyObjectByType<ReferenceDungeonMap>();
        if(map==null) return;
        var root=map.transform.Find("Castle Top Walls");
        if(root==null) return;
        const string path="Assets/Resources/CastleTiles.png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(importer==null) return;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var rects=provider.GetSpriteRects();
        var top=rects.FirstOrDefault(s=>s.name=="TopWall");
        if(top==null) return;
        // Use a one-tile-high brick strip rather than shrinking a tall wall section.
        top.rect=new Rect(16,512-270-16,48,16);
        provider.SetSpriteRects(rects);provider.Apply();importer.SaveAndReimport();
        foreach(var renderer in root.GetComponentsInChildren<SpriteRenderer>())
        {
            float worldWidth=renderer.size.x*Mathf.Abs(renderer.transform.lossyScale.x);
            var parentScale=renderer.transform.parent.lossyScale;
            renderer.transform.localScale=new Vector3(map.cellSize/Mathf.Abs(parentScale.x),map.cellSize/Mathf.Abs(parentScale.y),1);
            renderer.size=new Vector2(worldWidth/map.cellSize,1);
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(renderer.transform);
        }
        EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        if(EditorSceneManager.SaveScene(map.gameObject.scene)) File.WriteAllText(marker,"Wall sprites use consistent 16-pixel tiles and one-cell height.");
    }
}
