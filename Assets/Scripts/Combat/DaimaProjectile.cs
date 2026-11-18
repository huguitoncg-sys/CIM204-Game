using UnityEngine;
public class DaimaProjectile : MonoBehaviour
{
    private DaimaBoss boss;
    private Vector2 direction;
    private float speed,turn,life,delay;
    private bool stun, passedPlayer;
    public void Initialize(DaimaBoss owner,Vector2 heading,float velocity,float steering,bool blue,float wait=0)
    {boss=owner;direction=heading.normalized;speed=velocity;turn=steering;stun=blue;life=12;delay=wait;}
    private void Update()
    {
        if(boss==null||!boss.Fighting){Destroy(gameObject);return;}
        if(Time.timeScale<=0)return;
        if(delay>0){delay-=Time.deltaTime;return;}
        life-=Time.deltaTime;if(life<=0){Destroy(gameObject);return;}
        Vector2 p=transform.position;
        Vector2 desired=(Vector2)boss.Player.position-p;
        // Once the player is behind the shot, permanently commit to its current heading.
        if (Vector2.Dot(direction, desired) <= 0f) passedPlayer = true;
        if(turn>0 && !passedPlayer)direction=(Vector2)Vector3.RotateTowards(direction,desired.normalized,turn*Mathf.Deg2Rad*Time.deltaTime,0);
        Vector2 next=p+direction*speed*Time.deltaTime;
        // Swept distance prevents fast swords passing through the player between frames.
        Vector2 segment=next-p;
        float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot((Vector2)boss.Player.position-p,segment)/segment.sqrMagnitude):0;
        if(Vector2.Distance(p+segment*t,boss.Player.position)<(stun?.85f:.65f))
        {boss.HitPlayer(stun);Destroy(gameObject);return;}
        if (Vector2.Dot(direction, (Vector2)boss.Player.position - next) <= 0f) passedPlayer = true;
        transform.position=next;
        if(!boss.ProjectileBounds.Contains(next)){Destroy(gameObject);}
    }
}

