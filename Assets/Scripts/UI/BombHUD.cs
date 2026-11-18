using UnityEngine;
using UnityEngine.UI;

public class BombHUD : MonoBehaviour
{
    private PlayerBombs inventory;
    private Text label;
    public void Initialize(PlayerBombs owner)
    {
        inventory = owner;
        var health = FindAnyObjectByType<HealthHUD>();
        Text template = health != null ? health.GetComponent<Text>() : null;
        if (template == null)
        {
            foreach (var text in FindObjectsByType<Text>(FindObjectsSortMode.None))
                if (text.text.StartsWith("Health:")) { template = text; break; }
        }
        if (template != null)
        {
            var obj = new GameObject("Bomb Count", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(template.transform.parent, false);
            var rect = obj.GetComponent<RectTransform>();
            var source = template.rectTransform;
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.sizeDelta = source.sizeDelta;
            rect.localScale = source.localScale;
            rect.anchoredPosition = source.anchoredPosition +
                Vector2.down * Mathf.Max(source.rect.height, template.fontSize * 1.5f);
            label = obj.GetComponent<Text>();
            label.font = template.font;
            label.fontSize = template.fontSize;
            label.fontStyle = template.fontStyle;
            label.color = template.color;
            label.alignment = template.alignment;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false; GameUIFont.StyleHUD(label, 2);
        }
        inventory.CountChanged += Refresh;
        Refresh(inventory.BombCount);
    }
    private void Refresh(int count) { if (label != null) label.text = "Bombs: " + count; }
    private void OnDestroy()
    {
        if (inventory != null) inventory.CountChanged -= Refresh;
        if (label != null) Destroy(label.gameObject);
    }
}
