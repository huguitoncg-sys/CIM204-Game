using System.Collections.Generic;
using UnityEngine;

public class PlacedBomb : MonoBehaviour
{
    private float remaining=3f;
    private float radius;
    private bool exploded;
    private SpriteRenderer visual;
    private TextMesh countdown;
    private float flashRemaining;
    public void Initialize(Sprite circle,float blastRadius)
    {
        radius=blastRadius;
        visual=gameObject.AddComponent<SpriteRenderer>(); visual.sprite=circle; visual.color=Color.black; visual.sortingOrder=25;
        transform.localScale=Vector3.one*0.6f;
        var label=new GameObject("3-2-1 Countdown"); label.transform.SetParent(transform,false);
        label.transform.localPosition=new Vector3(0,1.15f,0);
        countdown=label.AddComponent<TextMesh>(); countdown.text="3"; countdown.anchor=TextAnchor.MiddleCenter;
        countdown.alignment=TextAlignment.Center; countdown.fontSize=64; countdown.characterSize=0.18f; countdown.color=Color.white;
        GameUIFont.StyleWorldText(countdown); label.GetComponent<MeshRenderer>().sortingOrder=30;
    }
    private void Update()
    {
        if (exploded)
        {
            flashRemaining-=Time.deltaTime;
            visual.color=new Color(1,0.55f,0.1f,Mathf.Clamp01(flashRemaining/0.22f)*0.6f);
            if(flashRemaining<=0) Destroy(gameObject);
            return;
        }
        remaining-=Time.deltaTime;
        countdown.text=Mathf.Clamp(Mathf.CeilToInt(remaining),1,3).ToString();
        if(remaining<=0) Explode();
    }
    private void Explode()
    {
        exploded=true;
        Destroy(countdown.gameObject);
        Physics2D.SyncTransforms();
        var hits=new List<Collider2D>();
        var filter=ContactFilter2D.noFilter; filter.useTriggers=true;
        Physics2D.OverlapCircle(transform.position,radius,filter,hits);
        var enemies=new HashSet<TriangleEnemy>();
        var walls=new HashSet<BombBreakableWall>();
        var bossTargets=new HashSet<BossCombatTarget>();
        bool hurtPlayer=false;
        foreach(var hit in hits)
        {
            var bossTarget=hit.GetComponentInParent<BossCombatTarget>();
            if(bossTarget!=null && bossTargets.Add(bossTarget)) bossTarget.TakeDamage(1);
            var enemy=hit.GetComponentInParent<TriangleEnemy>();
            if(enemy!=null && enemies.Add(enemy)) enemy.TakeDamage(1, (Vector2)(enemy.transform.position - transform.position));
            var wall=hit.GetComponentInParent<BombBreakableWall>();
            if(wall!=null && walls.Add(wall)) wall.Break();
            if(hit.GetComponentInParent<PlayerMovement>()!=null) hurtPlayer=true;
        }
        if(hurtPlayer)
        {
            var health=FindAnyObjectByType<HealthHUD>();
            if(health!=null) health.TakeDamage();
        }
        transform.localScale=Vector3.one*radius*2;
        flashRemaining=0.22f;
        visual.color=new Color(1,0.55f,0.1f,0.6f);
    }
}


