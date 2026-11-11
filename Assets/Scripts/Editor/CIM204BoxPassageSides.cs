using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CIM204BoxPassageSides
{
    static CIM204BoxPassageSides()=>EditorApplication.delayCall+=Apply;
    static void Apply()
    {
        const string marker="Library/CIM204BoxPassageSides.complete";
        if(File.Exists(marker))return;
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)
        {EditorApplication.delayCall+=Apply;return;}
        var puzzle=Object.FindAnyObjectByType<BoulderPuzzleRoom>();
        var brick=AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/CastleTiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Brick");
        if(puzzle==null||puzzle.map==null||brick==null)return;
        var map=puzzle.map;
        float gridLeft=puzzle.CellPosition(0).x-puzzle.tileSize/2;
        float gridRight=puzzle.CellPosition(4).x+puzzle.tileSize/2;
        float top=puzzle.CellPosition(0).y-puzzle.tileSize/2;
        float bottom=puzzle.CellPosition(30).y+puzzle.tileSize/2;
        float[] starts={map.Cell(17.5f,16).x,gridRight};
        float[] ends={gridLeft,map.Cell(23.5f,16).x};
        var material=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Maps/Dungeon Wall.physicsMaterial2D");
        for(int i=0;i<2;i++){
            float width=ends[i]-starts[i];
            if(width<=0)continue;
            var obj=new GameObject(i==0?"Box Passage Left Wall Fill":"Box Passage Right Wall Fill");
            obj.transform.SetParent(map.transform,false);
            obj.transform.position=new Vector3((starts[i]+ends[i])/2,(top+bottom)/2,0);
            obj.transform.localScale=Vector3.one*map.cellSize;
            var renderer=obj.AddComponent<SpriteRenderer>();
            renderer.sprite=brick;renderer.color=Color.white;
            renderer.drawMode=SpriteDrawMode.Tiled;renderer.tileMode=SpriteTileMode.Continuous;
            renderer.size=new Vector2(width,top-bottom)/map.cellSize;renderer.sortingOrder=-1;
            var collider=obj.AddComponent<BoxCollider2D>();collider.size=renderer.size;collider.sharedMaterial=material;
        }
        EditorSceneManager.MarkSceneDirty(puzzle.gameObject.scene);
        if(EditorSceneManager.SaveScene(puzzle.gameObject.scene))File.WriteAllText(marker,"Filled side strips outside the five-column puzzle grid.");
    }
}
