using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FirstRoomEncounter : MonoBehaviour
{
    public ReferenceDungeonMap map;
    public TriangleEnemy enemy;
    public GameObject[] yellowDoors;
    private PlayerMovement player;
    private PlayerMelee melee;
    private MapChestReward reward;
    private bool revealMapOnClose;
    private string popupMessage = "Defeat the enemy in the middle.\nuse wasd to move around.\nuse space to attack.";
    private bool shown, popupOpen, cleared, enemyAssigned;
    private bool movementWasEnabled, meleeWasEnabled;
    private float previousTimeScale;
    private GUIStyle messageStyle, buttonStyle;
    private Text hudText;
    private int popupOpenedFrame;
    private static bool popupBlocksInput;
    private static int consumedFrame = -1;
    public static bool BlocksGameplayInput => GameOverPopup.IsOpen || popupBlocksInput || consumedFrame == Time.frameCount;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInputState() { popupBlocksInput = false; consumedFrame = -1; }
    private readonly Rect room = new Rect(12.5f,4.5f,14f,8f);

    private void Start()
    {
        var hud = FindAnyObjectByType<CollectibleHUD>();
        if (hud != null) hudText = hud.GetComponentInChildren<Text>(true);
        player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) melee = player.GetComponent<PlayerMelee>();
        enemyAssigned = enemy != null;
        if (!enemyAssigned) Debug.LogError("First room encounter needs its enemy reference.", this);
    }
    private void Update()
    {
        if (GameOverPopup.IsOpen) return;
        if (popupOpen)
        {
            var keyboard = Keyboard.current;
            if (Time.frameCount > popupOpenedFrame && keyboard != null &&
                (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) ClosePopup();
            return;
        }
        if (cleared || map == null || player == null) return;
        if (enemyAssigned && (enemy == null || enemy.IsDefeated))
        {
            cleared = true;
            foreach (GameObject door in yellowDoors) if (door != null)
            {
                var castleDoor = door.GetComponent<CastleDoor>();
                if (castleDoor != null) castleDoor.Open(); else door.SetActive(false);
            }
            ClosePopup();
            SpawnChest();
            return;
        }
        if (!shown && room.Contains(map.Grid(player.transform.position)))
        {
            shown = popupOpen = true;
            popupOpenedFrame = Time.frameCount;
            popupBlocksInput = true;
            movementWasEnabled = player.enabled;
            meleeWasEnabled = melee != null && melee.enabled;
            player.enabled = false;
            if (melee != null) melee.enabled = false;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }
    }
    private void SpawnChest()
    {
        if (reward != null) return;
        var chestObject = new GameObject("Map Chest");
        chestObject.transform.SetParent(map.transform, false);
        reward = chestObject.AddComponent<MapChestReward>();
        reward.Initialize(map,player,this);
    }
    public void ShowMapReward()
    {
        if (popupOpen) return;
        popupMessage = "You have received a map";
        revealMapOnClose = true;
        popupOpen = true;
        popupOpenedFrame = Time.frameCount;
        popupBlocksInput = true;
        movementWasEnabled = player.enabled;
        meleeWasEnabled = melee != null && melee.enabled;
        player.enabled = false;
        if (melee != null) melee.enabled = false;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }
    public void ShowBombReward()
    {
        if (popupOpen) return;
        ShowMapReward();
        popupMessage = "You have received bombs.\nuse q to place bombs.";
        revealMapOnClose = false;
    }
    private void ClosePopup()
    {
        if (!popupOpen) return;
        popupOpen = false;
        popupBlocksInput = false;
        consumedFrame = Time.frameCount;
        Time.timeScale = previousTimeScale;
        if (player != null) player.enabled = movementWasEnabled;
        if (melee != null) melee.enabled = meleeWasEnabled;
        if (revealMapOnClose && reward != null) reward.RevealMap();
        revealMapOnClose = false;
    }
    private void OnDisable() => ClosePopup();
    private void OnGUI()
    {
        if (!popupOpen) return;
        if (messageStyle == null)
        {
            Font font = hudText != null ? hudText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            FontStyle style = hudText != null ? hudText.fontStyle : FontStyle.Bold;
            messageStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true, font = font, fontStyle = style };
            buttonStyle = new GUIStyle(GUI.skin.button) { font = font, fontStyle = style };
        }
        float height = Screen.height / 3f;
        float top = Screen.height - height;
        int fontSize = hudText != null ? Mathf.RoundToInt(hudText.fontSize * (hudText.canvas != null ? hudText.canvas.scaleFactor : 1f)) : Mathf.RoundToInt(Screen.height * 0.035f);
        messageStyle.fontSize = buttonStyle.fontSize = Mathf.Max(12, fontSize);
        GUI.depth = -100;
        Color previous = GUI.color;
        GUI.color = Color.white;
        GUI.Box(new Rect(0,top,Screen.width,height), GUIContent.none);
        GUI.Label(new Rect(20,top+height*0.03f,Screen.width-40,height*0.55f),popupMessage,messageStyle);
        float buttonWidth = Mathf.Min(180,Screen.width*0.3f);
        Rect buttonRect = new Rect((Screen.width-buttonWidth)/2,top+height*0.62f,buttonWidth,height*0.25f);
        if (GUI.Button(buttonRect,"ok",buttonStyle)) ClosePopup();
        GUI.color = previous;
    }
}








