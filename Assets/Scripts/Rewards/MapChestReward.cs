using UnityEngine;

public class MapChestReward : MonoBehaviour, IPlayerInteraction
{
    private ReferenceDungeonMap map;
    private PlayerMovement player;
    private PlayerInteraction interaction;
    private FirstRoomEncounter encounter;
    private SpriteRenderer chest;
    private Texture2D mapTexture, objectiveStar;
    private PlayerBombs bombInventory;
    private FillGridPuzzle tilePuzzle;
    private bool opened, mapVisible, nearby;

    public void Initialize(ReferenceDungeonMap dungeon, PlayerMovement owner, FirstRoomEncounter room)
    {
        map = dungeon; player = owner; encounter = room;
        interaction = player.GetComponent<PlayerInteraction>();
        if (interaction == null) interaction = player.gameObject.AddComponent<PlayerInteraction>();
        chest = gameObject.AddComponent<SpriteRenderer>();
        chest.sprite = CastleArt.Get("ChestClosed");
        chest.color = Color.white;
        chest.sortingOrder = 15;
        transform.position = map.Cell(19.5f,8.5f);
        transform.localScale = Vector3.one * 1.1f;
    }
    private void Update()
    {
        if (player == null || interaction == null) return;
        nearby = !opened && player.enabled && Time.timeScale > 0 &&
            ((Vector2)player.transform.position-(Vector2)transform.position).sqrMagnitude <= 1.8f*1.8f;
        if (nearby && interaction.Target == null) interaction.Target = this;
        else if (!nearby && ReferenceEquals(interaction.Target,this)) interaction.Target = null;
    }
    public void Interact()
    {
        if (opened || !nearby) return;
        opened = true;
        nearby = false;
        if (ReferenceEquals(interaction.Target,this)) interaction.Target = null;
        chest.sprite = CastleArt.Get("ChestOpen");
        encounter.ShowMapReward();
    }
    public void RevealMap()
    {
        if (mapVisible) return;
        var data = map.Data;
        mapTexture = new Texture2D(data.width,data.height,TextureFormat.RGBA32,false);
        mapTexture.name = "Dungeon Minimap";
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[data.width*data.height];
        for (int y=0;y<data.height;y++) for(int x=0;x<data.width;x++)
        {
            char cell=data.rows[y][x];
            pixels[(data.height-1-y)*data.width+x] = cell=='.' ? new Color(0.38f,0.42f,0.55f) : cell=='#' ? new Color(0.035f,0.04f,0.065f) : new Color(0.07f,0.075f,0.10f);
        }
        mapTexture.SetPixels(pixels); mapTexture.Apply(false,true);
        bombInventory = player.GetComponent<PlayerBombs>();
        tilePuzzle = FindAnyObjectByType<FillGridPuzzle>();
        objectiveStar = CreateStar();
        mapVisible = true;
    }
    private void OnGUI()
    {
        Color previous = GUI.color;
        GUI.color = Color.white;
        if (nearby) ChestPrompt.Draw();
        if (mapVisible && mapTexture != null && player != null)
        {
            GUI.depth = -20;
            float height = Mathf.Min(Screen.height*0.36f,Screen.width*0.25f*map.Data.height/map.Data.width);
            float width = height * map.Data.width/map.Data.height;
            Rect area = new Rect(Screen.width-width-20,20,width,height);
            GUI.Box(new Rect(area.x-6,area.y-6,width+12,height+12),GUIContent.none);
            GUI.DrawTexture(area,mapTexture,ScaleMode.StretchToFill,false);
            Vector2 objective = bombInventory == null || !bombInventory.IsUnlocked
                ? new Vector2(20.5f, 55.5f)
                : tilePuzzle == null || !tilePuzzle.IsSolved
                    ? new Vector2(38.5f, 19.5f)
                    : new Vector2(68.5f, 80f);
            float starSize = Mathf.Clamp(height * 0.075f, 18f, 30f);
            float starX = area.x + (objective.x + 0.5f) / map.Data.width * width;
            float starY = area.y + (objective.y + 0.5f) / map.Data.height * height;
            GUI.DrawTexture(new Rect(starX - starSize / 2, starY - starSize / 2, starSize, starSize), objectiveStar);
            Vector2 grid=map.Grid(player.transform.position);
            float px=area.x+Mathf.Clamp01((grid.x+0.5f)/map.Data.width)*width;
            float py=area.y+Mathf.Clamp01((grid.y+0.5f)/map.Data.height)*height;
            GUI.color=Color.black;
            GUI.DrawTexture(new Rect(px-5,py-5,10,10),Texture2D.whiteTexture);
            GUI.color=Color.white;
            GUI.DrawTexture(new Rect(px-3,py-3,6,6),Texture2D.whiteTexture);
        }
        GUI.color=previous;
    }
    private static Texture2D CreateStar()
    {
        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Objective Star";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Vector2[] outline = new Vector2[10];
        for (int i = 0; i < outline.Length; i++)
        {
            float angle = (90f + i * 36f) * Mathf.Deg2Rad;
            float radius = i % 2 == 0 ? 14f : 6f;
            outline[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            Vector2 point = new Vector2(x + 0.5f - size / 2f, y + 0.5f - size / 2f);
            Color color = InsideStar(point, outline)
                ? InsideStar(point / 0.78f, outline) ? new Color(1f, .83f, .12f) : new Color(.12f, .08f, .02f)
                : Color.clear;
            texture.SetPixel(x, y, color);
        }
        texture.Apply(false, true);
        return texture;
    }
    private static bool InsideStar(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[i], b = polygon[j];
            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
        }
        return inside;
    }
    private void OnDisable()
    {
        if (interaction != null && ReferenceEquals(interaction.Target,this)) interaction.Target=null;
    }
    private void OnDestroy()
    {
        if (mapTexture != null) Destroy(mapTexture);
        if (objectiveStar != null) Destroy(objectiveStar);

    }
}






