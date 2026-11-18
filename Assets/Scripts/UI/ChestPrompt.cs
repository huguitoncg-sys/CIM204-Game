using UnityEngine;
using UnityEngine.UI;

public static class ChestPrompt
{
    private static GUIStyle style;
    public static void Draw()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal };
            var health = Object.FindAnyObjectByType<HealthHUD>();
            var text = health != null ? health.GetComponent<Text>() : null;
            style.font = GameUIFont.Font;
            style.normal.textColor = Color.white;
        }
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.65f, 1.6f);
        style.fontSize = Mathf.RoundToInt(32 * scale);
        float width = Mathf.Min(Screen.width - 24, 400 * scale);
        float height = 72 * scale;
        GUI.Box(new Rect((Screen.width - width) / 2f, Screen.height - height - 24 * scale, width, height), "E - Open chest", style);
    }
}
