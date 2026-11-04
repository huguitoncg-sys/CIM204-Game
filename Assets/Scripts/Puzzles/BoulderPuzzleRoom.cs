using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BoulderPuzzleRoom : MonoBehaviour, IPlayerInteraction
{
    public ReferenceDungeonMap map;
    public Transform[] boulders;
    public float tileSize = 1.2f;
    private bool completedThisRun;
    private static readonly int[] InitialCells = {6,7,8,10,14,16,17,18,20,21,23,24};
    private readonly int[] cells = new int[12];
    private readonly Rect roomBounds = new Rect(17.5f,13.5f,7f,15f);
    private PlayerMovement movement;
    private PlayerRoomTransition transition;
    private PlayerInteraction interaction;
    private Rigidbody2D body;
    private int playerCell = 2;
    private Vector2Int facing = Vector2Int.down;
    private bool active, busy, fading, completedVisit;
    private bool wasMovement, wasTransition, wasSimulated;
    private CanvasGroup fade;
    private GameObject fadeObject;
    private string message = "Clear a path to the bottom exit.";
    private GUIStyle titleStyle, detailStyle;

    public Vector3 CellPosition(int index)
    {
        return transform.position + new Vector3((index % 5 - 2) * tileSize, -(index / 5) * tileSize, 0);
    }
    public void ResetBoulders()
    {
        if (completedThisRun) return;
        for (int i=0; i<InitialCells.Length; i++)
        {
            cells[i] = InitialCells[i];
            if (boulders != null && i < boulders.Length && boulders[i] != null)
                boulders[i].position = CellPosition(cells[i]);
        }
    }
    private void OnApplicationQuit()
    {
        completedThisRun = false;
        ResetBoulders();
    }
    private void Start()
    {
        completedThisRun = false;
        movement = FindAnyObjectByType<PlayerMovement>();
        if (movement == null || map == null) { enabled = false; return; }
        body = movement.GetComponent<Rigidbody2D>();
        transition = movement.GetComponent<PlayerRoomTransition>();
        interaction = movement.GetComponent<PlayerInteraction>();
        if (interaction == null) interaction = movement.gameObject.AddComponent<PlayerInteraction>();
        ResetBoulders();

    }
    private void Update()
    {
        if (GameOverPopup.IsOpen) return;
        if (movement == null || fading) return;
        bool inside = roomBounds.Contains(map.Grid(movement.transform.position));
        if (!active)
        {
            if (completedVisit)
            {
                if (!inside) StartCoroutine(ResetAfterRoomExit());
                return;
            }
            if (!completedThisRun && inside && movement.enabled) Enter();
            return;
        }
        if (!inside) { StartCoroutine(Leave(false)); return; }
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && !busy) { StartCoroutine(Leave(false)); return; }
        if (busy || Keyboard.current == null) return;
        var k = Keyboard.current;
        Vector2Int direction = Vector2Int.zero;
        if (k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) direction = Vector2Int.up;
        else if (k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame) direction = Vector2Int.down;
        else if (k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame) direction = Vector2Int.left;
        else if (k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame) direction = Vector2Int.right;
        if (direction == Vector2Int.zero) return;
        facing = direction;
        if (playerCell == 2 && facing == Vector2Int.up) { StartCoroutine(Leave(false)); return; }
        int next = Adjacent(playerCell, facing);
        if (!IsFloor(next)) { message = "That direction is blocked."; return; }
        if (BoulderAt(next) >= 0) { message = "Press E to push the boulder you are facing."; return; }
        StartCoroutine(Step(next, -1, -1));
    }
    private void Enter()
    {
        active = true;
        wasMovement = movement.enabled;
        wasTransition = transition != null && transition.enabled;
        wasSimulated = body.simulated;
        movement.enabled = false;
        if (transition != null) transition.enabled = false;
        body.linearVelocity = Vector2.zero;
        body.simulated = false;
        interaction.Target = this;
        ResetBoulders();
        playerCell = 2;
        facing = Vector2Int.down;
        PlacePlayer(CellPosition(playerCell));
        message = "Clear a path to the bottom exit.";
    }
    public void Interact()
    {
        if (!active || busy || fading) return;
        int next = Adjacent(playerCell, facing);
        int boulder = BoulderAt(next);
        if (boulder < 0) { message = "Face a nearby boulder, then press E."; return; }
        int destination = Adjacent(next, facing);
        if (!IsFloor(destination) || BoulderAt(destination) >= 0)
        { message = "There is no free space behind that boulder."; return; }
        StartCoroutine(Step(next, boulder, destination));
    }
    private static int Adjacent(int cell, Vector2Int direction)
    {
        int x = cell % 5 + direction.x;
        int y = cell / 5 - direction.y;
        return x < 0 || x >= 5 || y < 0 || y >= 7 ? -1 : y * 5 + x;
    }
    private static bool IsFloor(int cell) => cell == 2 || cell == 32 || (cell >= 5 && cell < 30);
    private int BoulderAt(int cell)
    {
        for (int i=0; i<cells.Length; i++) if (cells[i] == cell) return i;
        return -1;
    }
    private IEnumerator Step(int destination, int boulder, int boulderDestination)
    {
        busy = true;
        Vector3 from = movement.transform.position;
        Vector3 to = CellPosition(destination);
        Vector3 rockFrom = boulder >= 0 ? boulders[boulder].position : Vector3.zero;
        Vector3 rockTo = boulder >= 0 ? CellPosition(boulderDestination) : Vector3.zero;
        if (boulder >= 0) cells[boulder] = boulderDestination;
        playerCell = destination;
        float elapsed = 0;
        while (elapsed < 0.12f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 0.12f));
            PlacePlayer(Vector3.Lerp(from, to, t));
            if (boulder >= 0) boulders[boulder].position = Vector3.Lerp(rockFrom, rockTo, t);
            yield return null;
        }
        PlacePlayer(to);
        if (boulder >= 0) boulders[boulder].position = rockTo;
        busy = false;
        message = "Clear a path to the bottom exit.";
        if (playerCell == 32) yield return CompletePassage();
    }
    private IEnumerator CompletePassage()
    {
        completedThisRun = true;
        busy = true;
        Vector3 from = CellPosition(32);
        Vector3 to = from + Vector3.down * tileSize;
        float elapsed = 0;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            PlacePlayer(Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 0.15f))));
            yield return null;
        }
        PlacePlayer(to);
        completedVisit = true;
        RestoreControls();
        active = busy = false;
    }

    private IEnumerator ResetAfterRoomExit()
    {
        fading = true;
        wasMovement = movement.enabled;
        wasTransition = transition != null && transition.enabled;
        wasSimulated = body.simulated;
        movement.enabled = false;
        if (transition != null) transition.enabled = false;
        body.linearVelocity = Vector2.zero;
        body.simulated = false;
        CreateFade();
        yield return FadeTo(1);
        ResetBoulders();
        yield return FadeTo(0);
        RestoreControls();
        completedVisit = fading = false;
    }

    private void PlacePlayer(Vector3 position)
    {
        position.z = movement.transform.position.z;
        movement.transform.position = position;
        body.position = position;
    }
    private void CreateFade()
    {
        if (fade != null) return;
        fadeObject = new GameObject("Boulder Room Fade", typeof(Canvas), typeof(CanvasGroup));
        var canvas = fadeObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        fade = fadeObject.GetComponent<CanvasGroup>(); fade.alpha = 0;
        var screen = new GameObject("Black", typeof(RectTransform), typeof(Image));
        screen.transform.SetParent(fadeObject.transform, false);
        var rect = screen.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        screen.GetComponent<Image>().color = Color.black;
        screen.GetComponent<Image>().raycastTarget = false;
    }
    private IEnumerator FadeTo(float target)
    {
        float start = fade.alpha;
        float elapsed = 0;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/0.18f)));
            yield return null;
        }
        fade.alpha = target;
    }
    private IEnumerator Leave(bool solved)
    {
        if (fading) yield break;
        fading = busy = true;
        CreateFade();
        yield return FadeTo(1);
        ResetBoulders();
        PlacePlayer(solved ? map.Cell(25,28) : map.Cell(20.5f,12));
        // Allow the existing camera's smooth follow to settle while the screen is black.
        yield return new WaitForSecondsRealtime(0.3f);
        yield return FadeTo(0);
        RestoreControls();
        fading = busy = active = false;
    }
    private void RestoreControls()
    {
        if (movement != null) movement.enabled = wasMovement;
        if (body != null) { body.simulated = wasSimulated; body.linearVelocity = Vector2.zero; }
        if (transition != null) transition.enabled = wasTransition;
        if (interaction != null && ReferenceEquals(interaction.Target, this)) interaction.Target = null;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        if (active || fading) RestoreControls();
        if (fade != null) fade.alpha = 0;
        active = busy = fading = completedVisit = false;
        ResetBoulders();
    }
    private void OnDestroy() { if (fadeObject != null) Destroy(fadeObject); }
    private void OnGUI()
    {
        if (!active || fading) return;
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            detailStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.75f, 1.6f);
        titleStyle.fontSize = Mathf.RoundToInt(38 * scale);
        detailStyle.fontSize = Mathf.RoundToInt(26 * scale);
        float width = Mathf.Min(Screen.width - 24, Screen.width * 0.48f);
        float padding = 14 * scale;
        float contentWidth = width - padding * 2;
        string title = "Boulder Passage";
        string controls = "WASD / Arrows: step and face   |   E: push\nEsc: leave and reset";
        string status = message;
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

