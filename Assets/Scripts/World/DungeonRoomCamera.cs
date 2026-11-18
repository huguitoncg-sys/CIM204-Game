using UnityEngine;

public class DungeonRoomCamera : MonoBehaviour
{
    public ReferenceDungeonMap map;
    public Transform player;
    [UnityEngine.Serialization.FormerlySerializedAs("mazeZoom")]
    [SerializeField, Min(1f)] private float closeRoomZoom = 5f;
    [SerializeField, Min(0.01f)] private float transitionSmoothTime = 0.2f;
    private Vector3 positionVelocity;
    private float zoomVelocity;
    private bool initialized;
    // Circled rooms in the reference: entrance, start, maze and its chamber,
    // west halls and triangular passage, and the small side chambers.
    private readonly Rect[] closeRooms = {
        new Rect(3.5f, 5.5f, 9f, 6f),
        new Rect(11.5f, 3.5f, 16f, 10f),
        new Rect(30.5f, 1.5f, 23f, 15f),
        // Keep the maze zoom across its entrance and exit corridors too.
        new Rect(27.5f, 7.5f, 3f, 4f),
        new Rect(49.5f, 16.5f, 3f, 6f),
        new Rect(34.5f, 16.5f, 8f, 7f),
        new Rect(16.5f, 12.5f, 8f, 17f),
        new Rect(16.5f, 28.5f, 8f, 19f),
        new Rect(23.5f, 16.5f, 13f, 28f),
        new Rect(36.5f, 31.5f, 6f, 6f),
        new Rect(76.5f, 22.5f, 5f, 10f),
        new Rect(53.5f, 47.5f, 4f, 8f),
        new Rect(79.5f, 44.5f, 7f, 14f)
    };

    private bool UsesCloseZoom(Vector2 grid)
    {
        foreach (Rect room in closeRooms)
            if (room.Contains(grid)) return true;
        return false;
    }
    private Camera view;
    // Grid-space room bounds follow the reference drawing. Corridors use a closer following view.
    private readonly Rect[] rooms = {
        new Rect(4,6,9,6), new Rect(12,4,16,10), new Rect(17,13,8,17),
        new Rect(17,29,8,19), new Rect(24,17,13,28), new Rect(12,48,18,17),
        new Rect(31,2,23,15), new Rect(35,17,8,7), new Rect(37,32,6,6),
        new Rect(44,22,14,8), new Rect(62,17,15,22), new Rect(77,23,5,10),
        new Rect(60,43,18,18), new Rect(54,48,4,8), new Rect(80,45,7,14),
        new Rect(48,70,42,21), new Rect(65,91,8,7)
    };
    public void SnapAfterRespawn() { initialized = false; positionVelocity = Vector3.zero; zoomVelocity = 0; }
    private void Awake() => view = GetComponent<Camera>();
    private void LateUpdate()
    {
        if (map == null || player == null) return;
        Vector2 grid = map.Grid(player.position);
        // Follow the player at a fixed close zoom inside the selected rooms.
        if (UsesCloseZoom(grid))
        {
            ApplyCamera(player.position, closeRoomZoom);
            return;
        }
        Rect? chosen = null;
        foreach (Rect room in rooms)
            if (room.Contains(grid) && (!chosen.HasValue || room.width * room.height < chosen.Value.width * chosen.Value.height)) chosen = room;
        Vector3 center = player.position;
        float size = 7f;
        if (chosen.HasValue)
        {
            Rect room = chosen.Value;
            center = map.Cell(room.x + (room.width - 1) / 2f, room.y + (room.height - 1) / 2f);
            size = Mathf.Max(room.height * map.cellSize / 2, room.width * map.cellSize / (2 * view.aspect)) + 0.6f;
        }
        ApplyCamera(center, size);
    }
    private void ApplyCamera(Vector3 center, float size)
    {
        Vector3 target = new Vector3(center.x, center.y, transform.position.z);
        if (!initialized)
        {
            transform.position = target;
            view.orthographicSize = size;
            initialized = true;
            return;
        }
        transform.position = Vector3.SmoothDamp(transform.position, target,
            ref positionVelocity, transitionSmoothTime, Mathf.Infinity, Time.deltaTime);
        view.orthographicSize = Mathf.SmoothDamp(view.orthographicSize, size,
            ref zoomVelocity, transitionSmoothTime, Mathf.Infinity, Time.deltaTime);
    }}






