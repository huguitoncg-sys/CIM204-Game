using UnityEngine;
public class DaimaSpirit : BossCombatTarget
{
    private DaimaBoss boss;
    private bool ranged,dead;
    private float expires,nextShot,clock;
    private int shots,health=3;
    private SpriteRenderer visual;
    public void Initialize(DaimaBoss owner,bool shoots)
    {
        boss=owner;ranged=shoots;expires=Time.time+5;nextShot=Time.time+1;
        visual=gameObject.AddComponent<SpriteRenderer>();visual.sprite=boss.ghostFrames[0];visual.sortingOrder=20;
        gameObject.AddComponent<CircleCollider2D>().isTrigger=true;
        var body=gameObject.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.gravityScale=0;
    }
    private void Update()
    {
        if(dead)return;
        if(boss==null||!boss.Fighting){Destroy(gameObject);return;}
        if(Time.timeScale<=0)return;
        if(Time.time>=expires){Destroy(gameObject);return;}
        clock+=Time.deltaTime;visual.sprite=boss.ghostFrames[(int)(clock*6)%boss.ghostFrames.Length];
        if(ranged){

            if(Time.time>=nextShot&&shots<3){boss.Shoot(transform.position,((Vector2)(boss.Player.position-transform.position)).normalized,9,85,false,Color.white);shots++;nextShot=Time.time+1.5f;}
            if(shots==3&&Time.time>=nextShot){Destroy(gameObject);return;}
        }else{
            Vector2 next=Vector2.MoveTowards(transform.position,boss.Player.position,3.8f*Time.deltaTime);
            transform.position=next;
            if(Vector2.Distance(next,boss.Player.position)<.85f)boss.HitPlayer(false);
        }
    }
    public override void TakeDamage(int damage)
    {
        if(dead||damage<=0)return;
        health-=damage;DamageFlash.Play(gameObject,health<=0);
        if(health<=0){dead=true;gameObject.SetActive(false);Destroy(gameObject);}
    }
}

