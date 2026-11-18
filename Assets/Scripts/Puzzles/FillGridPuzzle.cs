using UnityEngine;
using UnityEngine.InputSystem;

public class FillGridPuzzle : MonoBehaviour
{
    public const int Columns = 6, Rows = 6, StartTile = 30, PlayableTiles = 30;
    // Rows are authored from top to bottom. # is stone; a space is outside the board.
    private static readonly string[] Layout = { ".....#", ".....#", "......", "..#...", "..#...", "#....#" };
    private static char CellKind(int index) => Layout[Rows - 1 - index / Columns][index % Columns];
    [SerializeField] private SpriteRenderer[] tiles;
    private readonly bool[] visited = new bool[Columns * Rows];
    private PlayerMovement player;
    private PlayerRoomTransition transition;
    private Rigidbody2D body;
    private RoomArea room;
    private Vector3 entryPosition;
    private bool active, waitForExit, solved, stuck;
    public bool IsSolved => solved;
    private bool previousMovement, previousTransition;
    private int current, filled;
    private GUIStyle titleStyle, detailStyle;
    private static readonly Color Empty = new Color(0.24f, 0.36f, 0.48f);
    private static readonly Color Filled = new Color(0.28f, 0.78f, 0.48f);

    public void Build(Sprite sprite)
    {
        if (tiles != null)
            foreach (var old in tiles)
                if (old != null)
                {
                    if (Application.isPlaying) { old.gameObject.SetActive(false); Destroy(old.gameObject); }
                    else DestroyImmediate(old.gameObject);
                }
        tiles = new SpriteRenderer[Columns * Rows];
        float spacing = 1.5f;
        for (int i = 0; i < tiles.Length; i++)
        {
            char kind = CellKind(i);
            if (kind == ' ') continue;
            GameObject tile = new GameObject(kind == '#' ? "Blocked Stone" : "Tile " + (i % Columns) + "," + (i / Columns));
            tile.transform.SetParent(transform, false);
            tile.transform.position = transform.position + new Vector3((i % Columns - 2.5f) * spacing, (i / Columns - 2.5f) * spacing, 0);
            tile.transform.localScale = new Vector3(1.42f / transform.lossyScale.x, 1.42f / transform.lossyScale.y, 1);
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
        if (GameOverPopup.IsOpen) return;
        if (room == null || tiles == null || tiles.Length != Columns * Rows) return;
        if (!active)
        {
            if (solved) return;
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
        if (!solved && k.rKey.wasPressedThisFrame) { Restart(); return; }
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
        solved = filled == PlayableTiles;
        if (solved)
        {
            var map = FindAnyObjectByType<ReferenceDungeonMap>();
            if (map != null) RespawnCheckpoint.Activate(2, map.Cell(38.5f,16f));
        }
        stuck = !solved && !CanStep(x + 1, y) && !CanStep(x - 1, y) && !CanStep(x, y + 1) && !CanStep(x, y - 1);
    }

    private bool CanStep(int x, int y)
    {
        if (x < 0 || x >= Columns || y < 0 || y >= Rows) return false;
        int index = y * Columns + x;
        return CellKind(index) == '.' && !visited[index];
    }

    private void PaintInitial()
    {
        if (tiles == null) return;
        for (int i = 0; i < tiles.Length; i++)
            if (tiles[i] != null) tiles[i].color = CellKind(i) == '#' ? new Color(0.40f, 0.46f, 0.58f) : i == StartTile ? Filled : Empty;
    }

    private void Restart()
    {
        if (solved) return;
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

    public void ExitForRespawn() { if (active) Leave(); }
    private void Leave()
    {
        if (solved)
        {
            Bounds bounds = tiles[0].bounds;
            foreach (var tile in tiles)
                if (tile != null) bounds.Encapsulate(tile.bounds);

            // Exit through the entrance before making the completed board solid.
            float halfHeight = player.GetComponent<Collider2D>().bounds.extents.y;
            MovePlayer(new Vector3(bounds.center.x, bounds.max.y + halfHeight + 0.15f, entryPosition.z));
            var barrier = new GameObject("Completed Puzzle Barrier");
            barrier.transform.SetParent(transform, false);
            barrier.transform.position = bounds.center;
            var collider = barrier.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(bounds.size.x / Mathf.Abs(transform.lossyScale.x),
                                        bounds.size.y / Mathf.Abs(transform.lossyScale.y));
        }
        else MovePlayer(entryPosition);
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
            titleStyle = new GUIStyle(GUI.skin.label) { font = GameUIFont.Font, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, wordWrap = true };
            detailStyle = new GUIStyle(GUI.skin.label) { font = GameUIFont.Font, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.75f, 1.6f);
        titleStyle.fontSize = Mathf.RoundToInt(32 * scale);
        detailStyle.fontSize = Mathf.RoundToInt(24 * scale);
        float width = Mathf.Min(Screen.width - 24, Screen.width * 0.48f);
        float padding = 14 * scale;
        float contentWidth = width - padding * 2;
        string title = solved ? "Puzzle complete! " + PlayableTiles + " / " + PlayableTiles : stuck ? "No moves left — press R to restart" : "Fill every tile: " + filled + " / " + PlayableTiles;
        string controls = solved ? "E: leave puzzle\nBoard completed and locked." : "WASD / Arrows: step\nR: restart   |   E: leave puzzle";
        string status = "";
        float titleHeight = titleStyle.CalcHeight(new GUIContent(title), contentWidth);
        float controlsHeight = detailStyle.CalcHeight(new GUIContent(controls), contentWidth);
        float statusHeight = string.IsNullOrEmpty(status) ? 0 : detailStyle.CalcHeight(new GUIContent(status), contentWidth) + padding * 0.5f;
        float height = titleHeight + controlsHeight + statusHeight + padding * 3;
        float left = (Screen.width - width) / 2;
        float top = 12 * scale;
        GUI.Box(new Rect(left, top, width, height), GUIContent.none);
        float y = top + padding;
        GUI.Label(new Rect(left + padding, y, contentWidth, titleHeight), title, titleStyle);
        y += titleHeight + padding * 0.5f;
        GUI.Label(new Rect(left + padding, y, contentWidth, controlsHeight), controls, detailStyle);
        y += controlsHeight + padding * 0.5f;
        if (!string.IsNullOrEmpty(status)) GUI.Label(new Rect(left + padding, y, contentWidth, statusHeight), status, detailStyle);
    }
}







