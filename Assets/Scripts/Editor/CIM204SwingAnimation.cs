using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204SwingAnimation
{
    static CIM204SwingAnimation()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204SwingAnimation.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var player=Object.FindAnyObjectByType<PlayerCharacterAnimation>();
        const string path="Assets/Sprites/Kurenai_Updated_Sprite.png";
        var importer=AssetImporter.GetAtPath(path) as TextureImporter;
        if(player==null||importer==null)return;
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Multiple;
        var walking=AssetImporter.GetAtPath("Assets/Sprites/Kurenai spritesheet.png") as TextureImporter;
        importer.spritePixelsPerUnit=walking!=null?walking.spritePixelsPerUnit:20;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency=true;
        var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        // Only the directional attack frames; the original movement sheet is untouched.
        int[] indices=Enumerable.Range(18,17).Prepend(17).ToArray();
        var rects=indices.Select(i=>new SpriteRect{name="Swing_"+i.ToString("D2"),
            rect=new Rect(i%6*96,576-(i/6+1)*96,96,96),
            pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()}).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply();importer.SaveAndReimport();
        var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
        System.Func<int,Sprite> frame=i=>sprites["Swing_"+i.ToString("D2")];
        player.ConfigureSwings(Enumerable.Range(18,4).Select(frame).ToArray(),
            Enumerable.Range(22,5).Select(frame).ToArray(),Enumerable.Range(27,3).Select(frame).ToArray(),
            Enumerable.Range(30,5).Select(frame).ToArray());
        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        if(EditorSceneManager.SaveScene(player.gameObject.scene))File.WriteAllText(marker,"Four directional swings configured.");
    }
}

