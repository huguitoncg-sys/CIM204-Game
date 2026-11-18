using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public static class GameUIFont
{
    private static Font pixelFont;
    public static Font Font => pixelFont != null ? pixelFont : (pixelFont = Resources.Load<Font>("Fonts/Minecraft"));
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        pixelFont = null;
        SceneManager.sceneLoaded -= ApplyScene;
        SceneManager.sceneLoaded += ApplyScene;
    }
    private static void ApplyScene(Scene scene, LoadSceneMode mode)
    {
        foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            text.font = Font;
            text.fontStyle = FontStyle.Normal;
            text.lineSpacing = 1.15f;
            int row = text.GetComponent<CollectibleHUD>() != null ? 0 :
                text.GetComponent<HealthHUD>() != null ? 1 : text.name == "Bomb Count" ? 2 : -1;
            if (row >= 0) StyleHUD(text, row);
        }
        foreach (var text in Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            StyleWorldText(text);
    }
    public static void StyleHUD(Text text, int row)
    {
        text.font = Font;
        text.fontSize = 40;
        text.fontStyle = FontStyle.Normal;
        text.resizeTextForBestFit = false;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        var rect = text.rectTransform;
        rect.localScale = Vector3.one;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(24, -20 - row * 58);
        rect.sizeDelta = new Vector2(430, 58);
    }
    public static void StyleWorldText(TextMesh text)
    {
        text.font = Font;
        text.fontStyle = FontStyle.Normal;
        text.GetComponent<MeshRenderer>().sharedMaterial = Font.material;
    }
}
