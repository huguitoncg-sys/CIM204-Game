using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    private bool collected;
    private float collectAfter;
    private Texture2D texture;
    private Sprite sprite;

    public static void Spawn(Vector3 position, float visualOffset = 0f)
    {
        var obj = new GameObject("Heart Pickup");
        obj.transform.position = position;
        var pickup = obj.AddComponent<HeartPickup>();
        pickup.collectAfter = Time.time + 0.25f;
        // A small pixel heart built independently of character artwork.
        string[] pixels = { ".##...##.", "####.####", "#########", "#########",
                            ".#######.", "..#####..", "...###...", "....#...." };
        pickup.texture = new Texture2D(9, 8, TextureFormat.RGBA32, false);
        pickup.texture.filterMode = FilterMode.Point;
        pickup.texture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 9; x++)
                pickup.texture.SetPixel(x, 7 - y, pixels[y][x] == '#' ? Color.white : Color.clear);
        pickup.texture.Apply();
        pickup.sprite = Sprite.Create(pickup.texture, new Rect(0, 0, 9, 8), new Vector2(0.5f, 0.5f), 12f);
        var art = new GameObject("Heart");
        art.transform.SetParent(obj.transform, false);
        art.transform.localPosition = new Vector3(visualOffset, 0.25f, 0);
        var renderer = art.AddComponent<SpriteRenderer>();
        renderer.sprite = pickup.sprite;
        renderer.color = new Color(1f, 0.15f, 0.25f);
        renderer.sortingOrder = 26;
        var trigger = obj.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.45f;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryCollect(other);
    private void OnTriggerStay2D(Collider2D other) => TryCollect(other);
    private void TryCollect(Collider2D other)
    {
        if (collected || Time.time < collectAfter || Time.timeScale <= 0) return;
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        var health = FindAnyObjectByType<HealthHUD>();
        // Consume the pickup even when healing is unnecessary; health stays capped at 3.
        if (health == null) return;
        health.RestoreHealth();
        collected = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (sprite != null) Destroy(sprite);
        if (texture != null) Destroy(texture);
    }
}


