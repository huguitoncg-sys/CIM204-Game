using UnityEngine;
[DefaultExecutionOrder(1000)]
public class PlayerStun : MonoBehaviour
{
    private float until;
    private SpriteRenderer visual;
    private Color normal;
    private bool tinted;
    public bool IsStunned => Time.time < until;
    private void Awake(){visual=GetComponent<SpriteRenderer>();normal=visual.color;}
    public void Clear(){until=0;tinted=false;if(visual!=null)visual.color=normal;}
    public void Apply(float seconds){until=Mathf.Max(until,Time.time+seconds);GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;}
    private void LateUpdate()
    {
        var flash=GetComponent<DamageFlash>();
        if(flash!=null && flash.IsFlashing)return;
        if(IsStunned){visual.color=new Color(.5f,.72f,1f,normal.a);tinted=true;}
        else if(tinted){visual.color=normal;tinted=false;}
    }
    private void OnDisable(){if(visual!=null)visual.color=normal;}
}

