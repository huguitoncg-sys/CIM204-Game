using UnityEngine;

public class MapChestReward : MonoBehaviour, IPlayerInteraction
{
    private ReferenceDungeonMap map;
    private PlayerMovement player;
    private PlayerInteraction interaction;
    private FirstRoomEncounter encounter;
    private SpriteRenderer chest;
    private Texture2D mapTexture;
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
    private void OnDisable()
    {
        if (interaction != null && ReferenceEquals(interaction.Target,this)) interaction.Target=null;
    }
    private void OnDestroy()
    {
        if (mapTexture != null) Destroy(mapTexture);

    }
}




