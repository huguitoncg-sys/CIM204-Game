using UnityEngine;

public class CastleDoor : MonoBehaviour
{
    [SerializeField] private Sprite openSprite;
    public void Configure(Sprite sprite) => openSprite = sprite;
    public void Open()
    {
        foreach (var collider in GetComponents<Collider2D>()) collider.enabled = false;
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null && openSprite != null) renderer.sprite = openSprite;
    }
}
