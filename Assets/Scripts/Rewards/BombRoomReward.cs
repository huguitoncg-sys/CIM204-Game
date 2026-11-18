using UnityEngine;

public class BombRoomReward : MonoBehaviour, IPlayerInteraction
{
    public ReferenceDungeonMap map;
    public TriangleEnemy[] enemies;
    private PlayerMovement player;
    private PlayerInteraction interaction;
    private PlayerBombs bombs;
    private FirstRoomEncounter popup;
    private GameObject chest;
    private Sprite chestSprite;
    private bool opened,nearby;
    private void Start()
    {
        player=FindAnyObjectByType<PlayerMovement>();
        if(player==null) { enabled=false; return; }
        interaction=player.GetComponent<PlayerInteraction>(); bombs=player.GetComponent<PlayerBombs>();
        popup=FindAnyObjectByType<FirstRoomEncounter>();
    }
    private void Update()
    {
        if(chest==null)
        {
            if(enemies==null || enemies.Length==0) return;
            foreach(var enemy in enemies) if(enemy!=null && !enemy.IsDefeated) return;
            chest=new GameObject("Bomb Chest"); chest.transform.SetParent(transform,false);
            chest.transform.position=map.Cell(20.5f,55.5f);
            chestSprite = CastleArt.Get("ChestClosed");
            var renderer=chest.AddComponent<SpriteRenderer>(); renderer.sprite=chestSprite;
            renderer.color=Color.white; renderer.sortingOrder=15;
        }
        nearby=!opened && player.enabled && Time.timeScale>0 && Vector2.Distance(player.transform.position,chest.transform.position)<=1.8f;
        if(nearby && interaction.Target==null) interaction.Target=this;
        else if(!nearby && ReferenceEquals(interaction.Target,this)) interaction.Target=null;
    }
    public void Interact()
    {
        if(!nearby || opened || bombs==null || popup==null) return;
        opened=true; nearby=false;
        if(ReferenceEquals(interaction.Target,this)) interaction.Target=null;
        chest.GetComponent<SpriteRenderer>().sprite=CastleArt.Get("ChestOpen");
        bombs.Unlock(); RespawnCheckpoint.Activate(1, map.Cell(22f,55.5f)); popup.ShowBombReward();
    }
    private void OnGUI()
    {
        if(nearby) ChestPrompt.Draw();
    }

    private void OnDisable()
    {
        if(interaction!=null && ReferenceEquals(interaction.Target,this)) interaction.Target=null;
    }
}




