using UnityEngine;
public class DaimaBoss : BossCombatTarget
{
    public ReferenceDungeonMap map;
    public Sprite[] frames,ghostFrames;
    public Sprite square,circle;
    public Transform Player {get;private set;}
    public bool Fighting {get;private set;}
    public Rect ProjectileBounds {get;private set;}
    private readonly Rect room=new Rect(47.5f,69.5f,42f,21f);
    private int health=25,attack;
    private bool defeated;
    private float nextAttack,nextTarget,clock,damageReady;
    private Vector2 destination;
    private SpriteRenderer visual;
    private Transform bar;
    private GameObject hazards;
    private HealthHUD playerHealth;
    private PlayerStun stun;
    private Texture2D projectileTexture;
    private Sprite projectileSprite;
    private void Start()
    {
        var player=FindAnyObjectByType<PlayerMovement>();if(player==null){enabled=false;return;}
        Player=player.transform;playerHealth=FindAnyObjectByType<HealthHUD>();
        stun=player.GetComponent<PlayerStun>();if(stun==null)stun=player.gameObject.AddComponent<PlayerStun>();
        visual=GetComponent<SpriteRenderer>();
        projectileTexture=new Texture2D(16,16,TextureFormat.RGBA32,false);
        projectileTexture.filterMode=FilterMode.Point;
        for(int y=0;y<16;y++)for(int x=0;x<16;x++)
            projectileTexture.SetPixel(x,y,new Vector2(x-7.5f,y-7.5f).sqrMagnitude<=56.25f?Color.white:Color.clear);
        projectileTexture.Apply();
        projectileSprite=Sprite.Create(projectileTexture,new Rect(0,0,16,16),new Vector2(.5f,.5f),16);
        circle=projectileSprite;
        Vector2 a=map.Cell(47,92),b=map.Cell(91,69);
        ProjectileBounds=Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        hazards=new GameObject("Daima Attacks");
        var backing=Part("Daima Health Background",transform,new Vector2(0,2.1f),new Vector2(3.6f,.28f),new Color(.1f,.05f,.05f));
        bar=Part("Daima Health 25",backing.transform,Vector2.zero,new Vector2(.96f,.7f),Color.red).transform;
    }
    private GameObject Part(string name,Transform parent,Vector2 position,Vector2 scale,Color color)
    {
        var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=new Vector3(scale.x,scale.y,1);
        var r=obj.AddComponent<SpriteRenderer>();r.sprite=square;r.color=color;r.sortingOrder=35;return obj;
    }
    public Vector2 RandomPoint()
    {
        for(int i=0;i<100;i++){Vector2 p=map.Cell(Random.Range(51f,86f),Random.Range(74f,87f));if(map.Walkable(p,Vector2.one*(1.6f*Mathf.Max(Mathf.Abs(transform.lossyScale.x),Mathf.Abs(transform.lossyScale.y)))))return p;}
        return map.Cell(68.5f,80);
    }
    private void Update()
    {
        if(defeated||Player==null||GameOverPopup.IsOpen)return;
        Vector2 grid=map.Grid(Player.position);
        if(grid.x>=65 && grid.x<=72 && grid.y>=67 && grid.y<=71)
            RespawnCheckpoint.Activate(3,map.Cell(55.5f,52.5f));
        bool inside=room.Contains(grid);
        if(!inside){
            if(Fighting){Fighting=false;ClearHazards();}
            return;
        }
        if(!Fighting){Fighting=true;nextAttack=Time.time+2;nextTarget=0;}
        if(Time.timeScale<=0)return;
        clock+=Time.deltaTime;visual.sprite=frames[(int)(clock*7)%frames.Length];visual.flipX=false;visual.flipY=false;
        if(Time.time>=nextTarget){destination=RandomPoint();nextTarget=Time.time+3;}
        Vector2 next=Vector2.MoveTowards(transform.position,destination,4f*Time.deltaTime);
        if(map.Walkable(next,Vector2.one*(1.4f*Mathf.Max(Mathf.Abs(transform.lossyScale.x),Mathf.Abs(transform.lossyScale.y)))))transform.position=next;else nextTarget=0;
        if(Time.time>=nextAttack){nextAttack=Time.time+10f;Attack(attack++%5);}
    }
    private void Attack(int type)
    {
        if(type==0){
            for(int i=0;i<5;i++){Vector2 heading=Quaternion.Euler(0,0,(i-2)*18)*((Vector2)(Player.position-transform.position)).normalized;Shoot(transform.position,heading,10,95,false,new Color(1,.65f,.1f));}
        }else if(type==1){
            Vector2 leftTop=map.Cell(51,73),rightBottom=map.Cell(86,88);
            int safeLane = Random.Range(1, 9); // Leave an interior gap in the horizontal volley.
            for(int i=0;i<10;i++){
                Sword(new Vector2(Mathf.Lerp(leftTop.x,rightBottom.x,(i+.5f)/10),leftTop.y),Vector2.down);
                if (i != safeLane) Sword(new Vector2(leftTop.x,Mathf.Lerp(leftTop.y,rightBottom.y,(i+.5f)/10)),Vector2.right);
            }
        }else if(type==2||type==3){
            int count=type==2?3:4;
            for(int i=0;i<count;i++){var spirit=new GameObject(type==2?"Ranged Spirit":"Melee Spirit");spirit.transform.SetParent(hazards.transform);spirit.transform.position=RandomPoint();spirit.AddComponent<DaimaSpirit>().Initialize(this,type==2);}
        }else Shoot(transform.position,((Vector2)(Player.position-transform.position)).normalized,3.2f,25,true,new Color(.3f,.65f,1));
    }
    public void Shoot(Vector2 position,Vector2 direction,float speed,float turn,bool blue,Color color)
    {
        var obj=new GameObject(blue?"Unblockable Stun Orb":"Homing Projectile");obj.transform.SetParent(hazards.transform);obj.transform.position=position;
        obj.transform.localScale=Vector3.one*(blue?1.1f:.35f);
        var r=obj.AddComponent<SpriteRenderer>();r.sprite=circle;r.color=color;r.sortingOrder=26;
        obj.AddComponent<DaimaProjectile>().Initialize(this,direction,speed,turn,blue);
    }
    private void Sword(Vector2 position,Vector2 direction)
    {
        var obj=new GameObject("Sword Volley");obj.transform.SetParent(hazards.transform);obj.transform.position=position;
        obj.transform.rotation=Quaternion.Euler(0,0,direction==Vector2.down?180:-90);
        Part("Blade",obj.transform,new Vector2(0,.35f),new Vector2(.2f,1.3f),new Color(.85f,.93f,1));
        Part("Guard",obj.transform,new Vector2(0,-.3f),new Vector2(.65f,.16f),new Color(1,.7f,.15f));
        Part("Grip",obj.transform,new Vector2(0,-.55f),new Vector2(.16f,.4f),new Color(.4f,.22f,.12f));
        obj.AddComponent<DaimaProjectile>().Initialize(this,direction,13,0,false,.6f);
    }
    public void HitPlayer(bool blue)
    {
        if(!Fighting||GameOverPopup.IsOpen||playerHealth==null||playerHealth.Health<=0)return;
        if(blue)stun.Apply(5);
        if(Time.time<damageReady)return;
        damageReady=Time.time+.8f;playerHealth.TakeDamage();

    }
    public override void TakeDamage(int damage)
    {
        if(!Fighting||defeated||damage<=0)return;
        health=Mathf.Max(0,health-damage);DamageFlash.Play(gameObject,health==0);
        if(bar!=null){bar.localScale=new Vector3(.96f*health/25f,.7f,1);bar.localPosition=new Vector3(-.48f*(1-health/25f),0,0);}
        if(health==0){defeated=true;Fighting=false;ClearHazards();gameObject.SetActive(false);}
    }
    public void ResetFight()
    {
        if(defeated)return;
        Fighting=false;health=25;attack=0;damageReady=0;nextTarget=0;
        transform.position=map.Cell(68.5f,78);
        if(bar!=null){bar.localScale=new Vector3(.96f,.7f,1);bar.localPosition=Vector3.zero;}
        ClearHazards();
    }
    private void ClearHazards(){if(hazards!=null)foreach(Transform child in hazards.transform)Destroy(child.gameObject);}
    private void OnDestroy(){if(hazards!=null)Destroy(hazards);if(projectileSprite!=null)Destroy(projectileSprite);if(projectileTexture!=null)Destroy(projectileTexture);}
}





