using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RoomArea : MonoBehaviour
{
    public RoomArea leftRoom;
    public RoomArea rightRoom;
    public Bounds Bounds => GetComponent<SpriteRenderer>().bounds;
}
