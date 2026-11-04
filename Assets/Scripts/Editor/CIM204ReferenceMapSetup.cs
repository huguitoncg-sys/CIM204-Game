using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CIM204ReferenceMapSetup
{
    static CIM204ReferenceMapSetup() => EditorApplication.delayCall += Setup;
    [MenuItem("CIM204/Build Reference Map")]
    public static void Setup()
    {
        const string marker = "Library/CIM204ReferenceMap.complete";
        if (File.Exists(marker)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Setup; return; }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
        Camera camera = Camera.main;
        TextAsset data = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Maps/ReferenceDungeon.json");
        Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerSquare.png");
        if (player == null || camera == null || data == null || square == null) return;
        if (Object.FindFirstObjectByType<ReferenceDungeonMap>() != null) return;
        string backup = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/Before Reference Map.unity");
        if (!EditorSceneManager.SaveScene(scene, backup, true)) throw new System.Exception("Could not back up scene.");
        GameObject archived = new GameObject("Previous Map (Backup)");
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == archived) continue;
            if (root.name.StartsWith("Background Rectangle") || root.GetComponentInChildren<TriangleEnemy>(true) != null || root.GetComponentInChildren<CircleCollectible>(true) != null)
                root.transform.SetParent(archived.transform, true);
        }
        archived.SetActive(false);
        GameObject mapObject = new GameObject("Green Dungeon - Reference Layout");
        ReferenceDungeonMap map = mapObject.AddComponent<ReferenceDungeonMap>();
        map.layoutAsset = data;
        var layout = map.Data;
        var physicsMaterial = new PhysicsMaterial2D("Dungeon Wall") { friction = 0, bounciness = 0 };
        AssetDatabase.CreateAsset(physicsMaterial, "Assets/Maps/Dungeon Wall.physicsMaterial2D");
        GameObject floors = new GameObject("Green Floors"); floors.transform.SetParent(mapObject.transform);
        GameObject walls = new GameObject("Solid Dark Walls"); walls.transform.SetParent(mapObject.transform);
        bool[,] used = new bool[layout.width, layout.height];
        for (int y = 0; y < layout.height; y++)
        for (int x = 0; x < layout.width; x++)
        {
            char type = layout.rows[y][x];
            if (type == ' ' || used[x,y]) continue;
            int width = 1;
            while (x + width < layout.width && !used[x+width,y] && layout.rows[y][x+width] == type) width++;
            int height = 1; bool extend = true;
            while (y + height < layout.height && extend)
            {
                for (int xx = 0; xx < width; xx++) if (used[x+xx,y+height] || layout.rows[y+height][x+xx] != type) { extend = false; break; }
                if (extend) height++;
            }
            for (int yy=0; yy<height; yy++) for (int xx=0; xx<width; xx++) used[x+xx,y+yy] = true;
            GameObject piece = new GameObject((type == '#' ? "Wall " : "Floor ") + x + "," + y);
            piece.transform.SetParent(type == '#' ? walls.transform : floors.transform);
            piece.transform.position = map.Cell(x + (width-1)/2f, y + (height-1)/2f);
            piece.transform.localScale = new Vector3(width * map.cellSize, height * map.cellSize, 1);
            SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = type == '#' ? new Color(0.035f,0.09f,0.055f) : new Color(0.10f,0.73f,0.66f);
            renderer.sortingOrder = type == '#' ? -5 : -10;
            if (type == '#') piece.AddComponent<BoxCollider2D>().sharedMaterial = physicsMaterial;
        }
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Triangle Enemy.prefab");
        GameObject enemies = new GameObject("Enemy Triangles"); enemies.transform.SetParent(mapObject.transform);
        if (enemyPrefab != null)
            foreach (var point in layout.enemies)
            {
                if (layout.rows[point.y][point.x] != '.') continue;
                GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
                enemy.transform.SetParent(enemies.transform);
                enemy.transform.position = map.Cell(point.x,point.y);
            }
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Circle Collectible.prefab");
        GameObject coins = new GameObject("Room Collectibles"); coins.transform.SetParent(mapObject.transform);
        if (coinPrefab != null)
            foreach (Vector2 point in new Vector2[] {new Vector2(8,9),new Vector2(39,35),new Vector2(79,28),new Vector2(55,52),new Vector2(83,52),new Vector2(68,94)})
            {
                GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab, scene);
                coin.transform.SetParent(coins.transform); coin.transform.position = map.Cell(point.x,point.y);
            }
        // Preserve the fill-grid puzzle in the small chamber directly below the maze.
        GameObject puzzle = new GameObject("Maze Reward - Grid Puzzle"); puzzle.transform.SetParent(mapObject.transform);
        puzzle.transform.position = map.Cell(38.5f,18.5f);
        puzzle.transform.localScale = new Vector3(9,6,1);
        var puzzleSprite = puzzle.AddComponent<SpriteRenderer>(); puzzleSprite.sprite = square;
        puzzleSprite.color = new Color(0.10f,0.73f,0.66f); puzzleSprite.sortingOrder = -9;
        puzzle.AddComponent<RoomArea>(); puzzle.AddComponent<FillGridPuzzle>().Build(square);
        foreach (Transform tile in puzzle.transform) { tile.position = puzzle.transform.position + (tile.position-puzzle.transform.position)*0.75f; tile.localScale *= 0.75f; }
        player.transform.position = map.Cell(19,9);
        var body = player.GetComponent<Rigidbody2D>(); body.position = player.transform.position;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var transition = player.GetComponent<PlayerRoomTransition>(); if (transition != null) transition.enabled = false;
        var playerSprite = player.GetComponent<SpriteRenderer>(); if (playerSprite != null) playerSprite.sortingOrder = 20;
        camera.backgroundColor = new Color(0.045f,0.12f,0.075f);
        camera.transform.position = new Vector3(0,0,-10);
        camera.orthographicSize = 8.5f;
        DungeonRoomCamera controller = camera.GetComponent<DungeonRoomCamera>();
        if (controller == null) controller = camera.gameObject.AddComponent<DungeonRoomCamera>();
        controller.map = map; controller.player = player.transform;
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        if (!EditorSceneManager.SaveScene(scene)) throw new System.Exception("Could not save reference map.");
        File.WriteAllText(marker, "Reference map generated; backup: " + backup);
        Selection.activeGameObject = mapObject;
        Debug.Log("CIM204: Green reference dungeon created with solid walls, connected rooms, room camera, enemies and puzzle. Backup: " + backup);
    }
}


