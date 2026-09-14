using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerRoomTransition : MonoBehaviour
{
    [SerializeField] private RoomArea currentRoom;
    [SerializeField] private Camera roomCamera;
    [SerializeField, Min(0.05f)] private float entryPadding = 0.3f;
    private Rigidbody2D body;
    private BoxCollider2D playerCollider;

    public void Configure(RoomArea startingRoom, Camera camera)
    {
        currentRoom = startingRoom;
        roomCamera = camera;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        if (roomCamera == null) roomCamera = Camera.main;
    }

    private void Start()
    {
        // New backgrounds and duplicates are discovered each time Play starts.
        foreach (SpriteRenderer sprite in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (sprite.gameObject.scene != gameObject.scene) continue;
            if (sprite.name.StartsWith("Background Rectangle") && sprite.GetComponent<RoomArea>() == null)
                sprite.gameObject.AddComponent<RoomArea>();
        }
        foreach (RoomArea room in FindObjectsByType<RoomArea>(FindObjectsSortMode.None))
        {
            if (room.gameObject.scene != gameObject.scene) continue;
            Bounds bounds = room.Bounds;
            Vector2 position = body.position;
            if (position.x >= bounds.min.x && position.x <= bounds.max.x &&
                position.y >= bounds.min.y && position.y <= bounds.max.y)
            {
                currentRoom = room;
                break;
            }
        }
        FrameRoom();
    }

    private void LateUpdate()
    {
        if (currentRoom == null) return;
        Bounds room = currentRoom.Bounds;
        Bounds player = playerCollider.bounds;
        if (player.max.x >= room.max.x && TryEnter(Vector2Int.right)) return;
        if (player.min.x <= room.min.x && TryEnter(Vector2Int.left)) return;
        if (player.max.y >= room.max.y && TryEnter(Vector2Int.up)) return;
        if (player.min.y <= room.min.y) TryEnter(Vector2Int.down);
    }

    private bool TryEnter(Vector2Int direction)
    {
        Bounds source = currentRoom.Bounds;
        Vector3 playerCenter = playerCollider.bounds.center;
        RoomArea nearest = null;
        float nearestGap = float.PositiveInfinity;
        // Ignore copied manual links: the layout determines the destination.
        foreach (RoomArea candidate in FindObjectsByType<RoomArea>(FindObjectsSortMode.None))
        {
            if (candidate == currentRoom || !candidate.isActiveAndEnabled ||
                candidate.gameObject.scene != gameObject.scene) continue;
            Bounds target = candidate.Bounds;
            bool horizontal = direction.x != 0;
            float transverse = horizontal ? playerCenter.y : playerCenter.x;
            float min = horizontal ? target.min.y : target.min.x;
            float max = horizontal ? target.max.y : target.max.x;
            if (transverse < min || transverse > max) continue;
            float gap = direction.x > 0 ? target.min.x - source.max.x :
                direction.x < 0 ? source.min.x - target.max.x :
                direction.y > 0 ? target.min.y - source.max.y : source.min.y - target.max.y;
            if (gap < -0.01f || gap >= nearestGap) continue;
            // A room must be large enough to place the whole player inside it.
            Vector3 extents = playerCollider.bounds.extents;
            if (target.extents.x <= extents.x + entryPadding || target.extents.y <= extents.y + entryPadding) continue;
            nearest = candidate;
            nearestGap = gap;
        }
        if (nearest == null) return false;
        Enter(nearest, direction);
        return true;
    }

    private void Enter(RoomArea destination, Vector2Int direction)
    {
        Bounds target = destination.Bounds;
        Bounds player = playerCollider.bounds;
        float insetX = player.extents.x + entryPadding;
        float insetY = player.extents.y + entryPadding;
        Vector2 center = new Vector2(
            Mathf.Clamp(player.center.x, target.min.x + insetX, target.max.x - insetX),
            Mathf.Clamp(player.center.y, target.min.y + insetY, target.max.y - insetY));
        if (direction.x > 0) center.x = target.min.x + insetX;
        if (direction.x < 0) center.x = target.max.x - insetX;
        if (direction.y > 0) center.y = target.min.y + insetY;
        if (direction.y < 0) center.y = target.max.y - insetY;
        Vector2 offset = (Vector2)player.center - (Vector2)transform.position;
        Vector2 position = center - offset;
        RigidbodyInterpolation2D interpolation = body.interpolation;
        body.interpolation = RigidbodyInterpolation2D.None;
        body.linearVelocity = Vector2.zero;
        body.position = position;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        Physics2D.SyncTransforms();
        body.interpolation = interpolation;
        currentRoom = destination;
        FrameRoom();
    }

    private void FrameRoom()
    {
        if (roomCamera == null || currentRoom == null) return;
        Bounds room = currentRoom.Bounds;
        roomCamera.transform.position = new Vector3(room.center.x, room.center.y, roomCamera.transform.position.z);
        if (roomCamera.orthographic)
            roomCamera.orthographicSize = Mathf.Max(room.extents.y, room.extents.x / roomCamera.aspect);
    }
}
