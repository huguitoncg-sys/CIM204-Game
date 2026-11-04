using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPopup : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    private Font font;
    private float openedAt;
    private bool ready, restarting;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState() => IsOpen = false;
    public void Show(Font hudFont)
    {
        if (IsOpen) return;
        font = hudFont;
        IsOpen = true;
        openedAt = Time.unscaledTime;
        Time.timeScale = 0f;
        var player = FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            player.enabled = false;
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null) body.linearVelocity = Vector2.zero;
        }
    }
    private void Update()
    {
        if (!IsOpen || restarting) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (!keyboard.eKey.isPressed && !keyboard.spaceKey.isPressed) ready = true;
        if (ready && Time.unscaledTime - openedAt > 0.25f &&
            (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) Restart();
    }
    private void LateUpdate() { if (IsOpen && !restarting) Time.timeScale = 0f; }
    private void Restart()
    {
        if (restarting) return;
        restarting = true;
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.path);
    }
    private void OnGUI()
    {
        if (!IsOpen || restarting) return;
        GUI.depth = -1000;
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.75f, 1.6f);
        var title = new GUIStyle(GUI.skin.label) { font = font, fontSize = Mathf.RoundToInt(38 * scale), alignment = TextAnchor.MiddleCenter };
        var button = new GUIStyle(GUI.skin.button) { font = font, fontSize = Mathf.RoundToInt(30 * scale) };
        var panel = new Rect(0, Screen.height * 2f / 3f, Screen.width, Screen.height / 3f);
        GUI.Box(panel, GUIContent.none);
        GUI.Label(new Rect(0, panel.y + 20 * scale, Screen.width, 70 * scale), "Game over", title);
        if (GUI.Button(new Rect((Screen.width - 220 * scale) / 2, panel.yMax - 95 * scale, 220 * scale, 60 * scale), "restart?", button))
            Restart();
    }
    private void OnDestroy()
    {
        if (IsOpen) { IsOpen = false; Time.timeScale = 1f; }
    }
}
