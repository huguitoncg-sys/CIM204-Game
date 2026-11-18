using UnityEngine;

public class RespawnCheckpoint : MonoBehaviour
{
    private int stage;
    private Vector3 spawn;
    private int bombsAtCheckpoint;
    public static void Activate(int checkpoint, Vector3 position)
    {
        var player=Object.FindAnyObjectByType<PlayerMovement>();
        if(player==null)return;
        var state=player.GetComponent<RespawnCheckpoint>();
        if(state==null)state=player.gameObject.AddComponent<RespawnCheckpoint>();
        if(checkpoint<=state.stage)return;
        state.stage=checkpoint;state.spawn=position;
        var bombs=player.GetComponent<PlayerBombs>();
        state.bombsAtCheckpoint=bombs!=null?bombs.BombCount:0;
    }
    public static bool Respawn()
    {
        var state=Object.FindAnyObjectByType<RespawnCheckpoint>();
        if(state==null||state.stage==0)return false;
        foreach(var puzzle in Object.FindObjectsByType<FillGridPuzzle>(FindObjectsSortMode.None))puzzle.ExitForRespawn();
        foreach(var boxes in Object.FindObjectsByType<BoulderPuzzleRoom>(FindObjectsSortMode.None))boxes.ExitForRespawn();
        foreach(var boss in Object.FindObjectsByType<DaimaBoss>(FindObjectsSortMode.None))boss.ResetFight();
        foreach(var bomb in Object.FindObjectsByType<PlacedBomb>(FindObjectsSortMode.None))Object.Destroy(bomb.gameObject);

        var flash=state.GetComponent<DamageFlash>();if(flash!=null)flash.Cancel();
        var stun=state.GetComponent<PlayerStun>();if(stun!=null)stun.Clear();
        var body=state.GetComponent<Rigidbody2D>();body.simulated=true;body.linearVelocity=Vector2.zero;
        state.transform.position=state.spawn;body.position=state.spawn;
        state.GetComponent<PlayerMovement>().enabled=true;
        var bombs=state.GetComponent<PlayerBombs>();if(bombs!=null)bombs.RestoreCheckpointCount(state.bombsAtCheckpoint);
        var health=Object.FindAnyObjectByType<HealthHUD>();if(health!=null)health.ResetHealth();
        var camera=Object.FindAnyObjectByType<DungeonRoomCamera>();if(camera!=null)camera.SnapAfterRespawn();
        Physics2D.SyncTransforms();
        return true;
    }
}

