using UnityEngine;
using UnityEngine.InputSystem;

public class FillGridPuzzle : MonoBehaviour
{
    public const int Columns = 7, Rows = 6, Blocked = 3, StartTile = 38;
    [SerializeField] private SpriteRenderer[] tiles;
    private readonly bool[] visited = new bool[Columns * Rows];
    private PlayerMovement player;
    private PlayerRoomTransition transition;
    private Rigidbody2D body;
    private RoomArea room;
    private Vector3 entryPosition;
    private bool active, waitForExit, solved, stuck;
    private bool previousMovement, previousTransition;
    private int current, filled;
    private GUIStyle titleStyle, detailStyle;
    private static readonly Color Empty = new Color(0.24f, 0.36f, 0.48f);
    private static readonly Color Filled = new Color(0.28f, 0.78f, 0.48f);

    public void Build(Sprite sprite)
    {
        tiles = new SpriteRenderer[Columns * Rows];
        for (int i = 0; i < tiles.Length; i++)
        {
            GameObject tile = new GameObject(i == Blocked ? "Blocked Tile" : "Tile " + (i % Columns) + "," + (i / Columns));
            tile.transform.SetParent(transform, false);
            // World-space sizes are independent of the background's scale.
            tile.transform.position = transform.position + new Vector3((i % Columns - 3) * 1.15f, (i / Columns - 2.5f) * 1.15f - 0.3f, 0);
            tile.transform.localScale = new Vector3(1.05f / transform.lossyScale.x, 1.05f / transform.lossyScale.y, 1);
            tiles[i] = tile.AddComponent<SpriteRenderer>();
            tiles[i].sprite = sprite;
            tiles[i].sortingOrder = 10;
        }
        PaintInitial();
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>();
        if (player == null) { enabled = false; return; }
        body = player.GetComponent<Rigidbody2D>();
        transition = player.GetComponent<PlayerRoomTransition>();
        room = GetComponent<RoomArea>();
        PaintInitial();
    }

    private bool InsideRoom()
    {
        Bounds bounds = room.Bounds;
        Vector2 p = body.position;
        return p.x > bounds.min.x && p.x < bounds.max.x && p.y > bounds.min.y && p.y < bounds.max.y;
    }

    private void Update()
    {
        if (room == null || tiles == null || tiles.Length != 42) return;
        if (!active)
        {
            if (!InsideRoom()) { waitForExit = false; return; }
            if (waitForExit) return;
            entryPosition = player.transform.position;
            previousMovement = player.enabled;
            previousTransition = transition != null && transition.enabled;
            player.enabled = false;
            if (transition != null) transition.enabled = false;
            active = true;
            Restart();
        }
        Keyboard k = Keyboard.current;
        if (k == null) return;
        if (k.eKey.wasPressedThisFrame) { Leave(); return; }
        if (k.rKey.wasPressedThisFrame) { Restart(); return; }
        if (solved || stuck) return;
        int x = current % Columns, y = current / Columns;
        if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame) y++;
        else if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame) y--;
        else if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame) x--;
        else if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame) x++;
        else return;
        if (!CanStep(x, y)) return;
        current = y * Columns + x;
        visited[current] = true;
        filled++;
        tiles[current].color = Filled;
        MovePlayer(tiles[current].transform.position);
        solved = filled == Columns * Rows - 1;
        stuck = !solved && !CanStep(x + 1, y) && !CanStep(x - 1, y) && !CanStep(x, y + 1) && !CanStep(x, y - 1);
    }

    private bool CanStep(int x, int y)
    {
        if (x < 0 || x >= Columns || y < 0 || y >= Rows) return false;
        int index = y * Columns + x;
        return index != Blocked && !visited[index];
    }

    private void PaintInitial()
    {
        if (tiles == null) return;
        for (int i = 0; i < tiles.Length; i++)
            if (tiles[i] != null) tiles[i].color = i == Blocked ? new Color(0.08f, 0.1f, 0.14f) : i == StartTile ? Filled : Empty;
    }

    private void Restart()
    {
        System.Array.Clear(visited, 0, visited.Length);
        current = StartTile;
        visited[current] = true;
        filled = 1;
        solved = stuck = false;
        PaintInitial();
        MovePlayer(tiles[current].transform.position);
    }

    private void MovePlayer(Vector3 position)
    {
        body.linearVelocity = Vector2.zero;
        body.position = position;
        player.transform.position = new Vector3(position.x, position.y, player.transform.position.z);
        Physics2D.SyncTransforms();
    }

    private void Leave()
    {
        MovePlayer(entryPosition);
        player.enabled = previousMovement;
        if (transition != null) transition.enabled = previousTransition;
        active = false;
        waitForExit = true;
    }

    private void OnDisable()
    {
        if (active && player != null) Leave();
    }

    private void OnGUI()
    {
        if (!active) return;
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 22, fontStyle = FontStyle.Bold };
            detailStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 15 };
        }
        float width = Mathf.Min(620, Screen.width - 20);
        float left = (Screen.width - width) / 2;
        GUI.Box(new Rect(left, 12, width, 86), GUIContent.none);
        GUI.Label(new Rect(left, 17, width, 32), solved ? "Puzzle complete! 41 / 41" : stuck ? "No moves left — press R to restart" : "Fill every tile: " + filled + " / 41", titleStyle);
        GUI.Label(new Rect(left, 52, width, 32), "WASD / Arrows: step   |   R: restart   |   E: leave puzzle", detailStyle);
    }
}
